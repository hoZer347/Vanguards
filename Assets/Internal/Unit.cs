using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Vanguards
{
	public class Unit : MonoBehaviour
	{
		public bool ActionUsed
		{
			get => spriteHost.IsGrayScale;
			set
			{
				spriteHost.IsGrayScale = value;
				spriteHost.SetFrameRate(value ? 2 : 4);
			}
		}

		static public bool CheckForTurnEnd()
		{
			Unit[] units = GameObject.FindObjectsByType<Unit>(FindObjectsSortMode.None);

			foreach (Unit unit in units)
				if (unit.ActionUsed == false &&
					unit.team == eTeam.Player)
					return false;

			return true;
		}

		#region Attributes

		public int MaxHP => HP.Base;
		public int CurrentHP => HP.Value;

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

		[HideInInspector] public Attribute<string> NAME = new();
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

		public void AddToPath(Vector3 position)
		{
			path.Add(position);
		}

		public void SetPath(IEnumerable<Vector3> path)
		{
			this.path.Clear();
			this.path.AddRange(path);
		}

		public Vector2Int coords =>
			new Vector2Int(
				(int)transform.position.x,
				(int)transform.position.z);

		SpriteHost spriteHost;

        #region Animation State Management

        public enum eState
		{
			Idle,
			Moving,
			Attacking,
		};

		[SerializeField]
		eState state = eState.Idle;
		public eState State => state;

		public void SetState(eState state)
		{
			this.state = state;
			switch (state)
			{
				case eState.Idle:
					SetAnimationState(eAnimationState.Idle);
					break;
			};
		}

		public enum eAnimationState
		{
			Idle = 0,
			Attacking = 3,
			Moving_Left = 6,
			Moving_Right = 9,
			Moving_Down = 12,
			Moving_Up = 15,
			Moving_Left_Down = 18,
			Moving_Right_Down = 21,
			Moving_Left_Up = 24,
			Moving_Right_Up = 27,
		};

		[SerializeField]
		eAnimationState animationState = eAnimationState.Idle;
		eAnimationState AnimationState => animationState;

		public void SetAnimationState(eAnimationState animationState)
		{
			this.animationState = animationState;
			spriteHost.SetOffset((int)animationState + (int)team);
		}

		void HandleAnimationState()
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
		eTeam team = eTeam.Player;
		public eTeam Team => team;

		public void SetTeam(eTeam team)
		{
			this.team = team;
			spriteHost.SetOffset((int)animationState + (int)team);
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

		void SetMovementType(eMovementType movementType)
		{
			//switch (movementType)
			//{
			//	case eMovementType.Land:
			//		ff_Kernel = (Cell cell, float amount, out bool shouldAdd) =>
			//		{
			//			if (MoveRange <= amount)
			//			{
			//				shouldAdd = true;

			//				return amount -= cell.Difficulty;
			//			}
			//			else if (amount - MoveRange < AttackRange)
			//			{
			//				shouldAdd = true;

			//				return amount - 1;
			//			}
			//			else if (amount - MoveRange < StaffRange)
			//			{
			//				shouldAdd = true;

			//				return amount - 1;
			//			};

			//			shouldAdd = false;

			//			return amount - 1;
			//		};

			//		break;
			//	case eMovementType.Flying:
			//		ff_Kernel =
			//			(Cell cell, float amount, out bool shouldAdd) =>
			//			{
			//				shouldAdd = true;

			//				return amount -= cell.Difficulty;
			//			};
			//		break;
			//};
		}

		Algorithm.FloodFillHeuristic ff_Kernel =
			(Cell cell, float amount) =>
			{
				if (cell.Unit != null && cell.Unit.Team == eTeam.Enemy)
						return 0.0f;

				return amount - cell.Difficulty;
			};

		public Algorithm.FloodFillHeuristic FF_Kernel => ff_Kernel;

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

		[SerializeField]
		List<Vector3> path = new();
		public List<Vector3> Path => path;

		#endregion

		#region Refresh

		private void Start() => Refresh();
		private void OnValidate() => Refresh();

		void Refresh()
		{
			spriteHost = GetComponent<SpriteHost>();
			
			SetState(state);
			SetAnimationState(animationState);
			SetTeam(team);
			SetMovementType(movementType);

			name = NAME.Base;

			if (equipped != null)
				equipped.transform.SetAsLastSibling();
		}

		#endregion

		[SerializeField]
		float speed = 1;

		[SerializeField]
		float turnThreshold = 0.5f;

		private void Update()
		{
			HandleAnimationState();

			transform.rotation =
				Quaternion.LookRotation(
					Quaternion.Euler(
						Camera.main.transform.eulerAngles) * Vector3.forward,
				Vector3.up);
		}

		#region GUI

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

		void DoGUI()
		{
			AttributeGUI.DoAttributesGUI(this);
		}

		[CustomEditor(typeof(Unit))]
		public class UnitEditor : Editor
		{
			override public void OnInspectorGUI()
			{
				base.OnInspectorGUI();

				Unit unit = (Unit)target;

				unit.DoGUI();
			}
		}

#endif

		#endregion
	};
};
