using System.Collections.Generic;
using UnityEngine;
using  System.Linq;


namespace Vanguards
{
	public class Unit : MonoBehaviour
	{
		SpriteHost spriteManager;

		#region State Management

		public enum eState
		{
			Idle,
			Moving,
			Attacking,
		};

		[SerializeField]
		eState state = eState.Idle;
		eState State => state;

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

		#endregion

		#region Animation State Management

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
			spriteManager.SetOffset((int)animationState + (int)team);
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
		eTeam Team => team;

		public void SetTeam(eTeam team)
		{
			this.team = team;
			spriteManager.SetOffset((int)animationState + (int)team);
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
			switch (movementType)
			{
				case eMovementType.Land:
					movementFFKernel = (Cell cell, float amount, out bool shouldAdd) =>
					{
						shouldAdd = true;

						return amount -= cell.Difficulty;
					};
					break;

				case eMovementType.Flying:
					movementFFKernel =
						(Cell cell, float amount, out bool shouldAdd) =>
						{
							shouldAdd = true;

							return amount -= cell.Difficulty;
						};
					break;
			};
		}

		// TODO: Change this based on Cell properties
		Map.FloodFillKernel movementFFKernel =
			(Cell cell, float amount, out bool shouldAdd) =>
			{
				shouldAdd = true;
				return amount -= cell.Difficulty;
			};
		public Map.FloodFillKernel MovementFFKernel => movementFFKernel;

		#endregion

		private void Start() => Refresh();
		private void OnValidate() => Refresh();

		void Refresh()
		{
			spriteManager = GetComponent<SpriteHost>();

			SetState(state);
			SetAnimationState(animationState);
			SetTeam(team);
			SetMovementType(movementType);

			LockToCell();
		}

		[EasyButtons.Button]
		public void LockToCell()
		{
			if (Map.main == null) return;

			Cell cell =
				Map.main[
					new Vector2Int(
						(int)transform.position.x,
						(int)transform.position.z)];

			if (cell != null)
				transform.position = cell.Position;
		}

		[SerializeField]
		List<Vector3> path = new();

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

		[SerializeField]
		float speed = 1;

		[SerializeField]
		float turnThreshold = 0.5f;

		private void Update()
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


			transform.rotation =
				Quaternion.LookRotation(
					Quaternion.Euler(
						Camera.main.transform.eulerAngles) * Vector3.forward,
				Vector3.up);
		}
	};
};
