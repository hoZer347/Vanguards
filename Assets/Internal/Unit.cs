using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vanguards
{
	public class Unit : MonoBehaviour
	{
		#region Game Logic

		public bool ActionUsed
		{
			//get => spriteHost.IsGrayScale;
			get => false;
			set
			{
				//if (material == null)
				//	material = GetComponent<Material>();
			}
		}

		static public bool CheckForTurnEnd()
		{
			Unit[] units = GameObject.FindObjectsByType<Unit>(FindObjectsSortMode.None);

			foreach (Unit unit in units)
				if (unit.ActionUsed == false &&
					unit.Team == eTeam.Player)
					return false;

			return true;
		}

		#endregion

		#region Attributes

		[HideInInspector] public Attribute<int> HP = new();
		[HideInInspector] public Attribute<int> STR = new();
		[HideInInspector] public Attribute<int> MAG = new();
		[HideInInspector] public Attribute<int> SPD = new();
		[HideInInspector] public Attribute<int> SKL = new();
		[HideInInspector] public Attribute<int> LCK = new();
		[HideInInspector] public Attribute<int> DEF = new();
		[HideInInspector] public Attribute<int> RES = new();
		[HideInInspector] public Attribute<int> MOV = new();

		public Equippable equipped = null;

		#endregion

		#region Equipment Management

		public (int, int) GetAttackRange()
		{
			int max = 0, min = int.MaxValue;

			foreach (Weapon weapon in GetComponentsInChildren<Weapon>())
			{
				min = Mathf.Min(min, weapon.MIN_RNG.Value);
				max = Mathf.Max(max, weapon.RNG.Value);
			};

			return (min, max);
		}

		public (int, int) GetStaffRange()
		{
			int max = 0, min = int.MaxValue;

			foreach (Staff weapon in GetComponentsInChildren<Staff>())
			{
				min = Mathf.Min(min, weapon.MIN_RNG.Value);
				max = Mathf.Max(max, weapon.RNG.Value);
			};

			return (min, max);
		}

		public bool HasItem<_ItemType>()
			=> GetComponentInChildren<_ItemType>() != null;

		public int MoveRange => MOV.Value;

		public bool hasStaff => GetComponentInChildren<Staff>() != null;
		public bool hasAttack => GetComponentInChildren<Weapon>() != null;

		#endregion

        #region Animation State Management

		public enum eAnimationState
		{
			Idle,
			Attacking,
			Moving_Left,
			Moving_Right,
			Moving_Down,
			Moving_Up,
			Moving_Left_Down,
			Moving_Right_Down,
			Moving_Left_Up,
			Moving_Right_Up,
		};

		[SerializeField]
		eAnimationState animationState = eAnimationState.Idle;

		MeshRenderer meshRenderer;

		public void UpdateSpriteIndex()
		{
			Vector2 index = new Vector2(
				(int)team,
				(int)animationState + 1);

			MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

			if (meshRenderer == null)
				return;

			if (Application.isPlaying)
				meshRenderer.material.SetVector("_Index", index);
			
			else
			{
				Material mat = meshRenderer.sharedMaterial;
				if (mat != null)
				{
#if UNITY_EDITOR
					Undo.RecordObject(mat, "Set Sprite Index");
#endif
					mat.SetVector("_Index", index);
				};
			};
		}

		public void SetAnimationState(eAnimationState animationState)
		{
			this.animationState = animationState;
			//UpdateSpriteIndex();
		}

		public void HandleAnimationState()
		{
			if (path.Count > 0)
			{
				Vector3 delta = path[0] - transform.position + Vector3.up * 0.001f;

				if (Vector3.Distance(path[0], transform.position) < speed * Time.deltaTime)
				{
					transform.position = path[0];
					path.RemoveAt(0);

					SetAnimationState(eAnimationState.Idle);
				}
				else transform.position += delta.normalized * speed * Time.deltaTime;

				delta =
					Quaternion.Inverse(
						Camera.main.transform.rotation) * delta;

				if (delta.x < -turnThreshold)
				{
					if (delta.z < -turnThreshold)
						SetAnimationState(eAnimationState.Moving_Left_Down);
					else if (delta.z > turnThreshold)
						SetAnimationState(eAnimationState.Moving_Left_Up);
					else
						SetAnimationState(eAnimationState.Moving_Left);
				}
				else if (delta.x > turnThreshold)
				{
					if (delta.z < -turnThreshold)
						SetAnimationState(eAnimationState.Moving_Right_Down);
					else if (delta.z > turnThreshold)
						SetAnimationState(eAnimationState.Moving_Right_Up);
					else
						SetAnimationState(eAnimationState.Moving_Right);
				}
				else
				{
					if (delta.z < -turnThreshold)
						SetAnimationState(eAnimationState.Moving_Down);
					else if (delta.z > turnThreshold)
						SetAnimationState(eAnimationState.Moving_Up);
				};
			}
			else SetAnimationState(eAnimationState.Idle);
			
			UpdateSpriteIndex();
		}

		#endregion


		#region Team Management

		public enum eTeam
		{
			Enemy = 0,
			Player = 1,
			Ally = 2,
		};

		[SerializeField]
		eTeam team;

		public eTeam Team => team;

		public void SetTeam(eTeam team)
		{
			this.team = team;
			//UpdateSpriteIndex();
		}

		#endregion

		#region Movement

		public enum eMovementType
		{
			Land,
			Flying,
			Mounted,
		};

		[SerializeField]
		eMovementType movementType = eMovementType.Land;
		public eMovementType MovementType => movementType;

		Algorithm.FloodFillHeuristic ff_Kernel =
			(Cell cell, float amount) =>
			{
				if (cell.Unit != null && cell.Unit.Team == eTeam.Enemy)
					return 0.0f;

				return amount - cell.Difficulty;
			};

		public Algorithm.FloodFillHeuristic FF_Kernel => ff_Kernel;

		List<Vector3> path = new();
		public List<Vector3> Path => path;

		[SerializeField]
		float speed = 1;

		[SerializeField]
		float turnThreshold = 0.5f;

		public void AddToPath(Vector3 position)
			=> path.Add(position);

		public void SetPath(IEnumerable<Vector3> path)
		{
			this.path.Clear();
			this.path.AddRange(path);
		}

		public Vector2Int coords =>
			new Vector2Int(
				(int)transform.position.x,
				(int)transform.position.z);

		void SetMovementType(eMovementType movementType)
		{

		}

		public void LockToCell()
		{
			if (Map.main == null)
				return;

			Cell cell =
				Map.main[
					new Vector2Int(
						(int)transform.position.x,
						(int)transform.position.z)];

			if (cell != null)
				transform.position = cell.Position;

			else transform.position =
					new Vector3(
						transform.position.x,
						0,
						transform.position.z);
		}

		#endregion

		#region Refresh

		private void Start() => Refresh();
		private void OnValidate() => Refresh();

		void Refresh()
		{
			SetAnimationState(animationState);
			SetMovementType(movementType);
			SetTeam(Team);

			if (equipped != null)
				equipped.transform.SetAsLastSibling();

			UpdateSpriteIndex();
		}

		#endregion

		#region Editor GUI

#if UNITY_EDITOR

		bool showPath = true;
		private void OnDrawGizmos()
		{
			if (showPath)
			{
				Gizmos.color = Color.green;

				foreach (Vector3 position in path)
					Gizmos.DrawSphere(
						position,
						0.4f);
			};
		}

		void DoGUI() => AttributeGUI.DoAttributesGUI(this);

		[CustomEditor(typeof(Unit))]
		public class UnitEditor : Editor
		{
			override public void OnInspectorGUI()
			{
				Unit unit = (Unit)target;

				base.OnInspectorGUI();

				unit.DoGUI();
			}
		}

#endif

		#endregion
	};
};
