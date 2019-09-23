namespace Common.Activatable
{
	public enum ActivatableState
	{
		Inactive,
		Active,
		ToActive,
		ToInactive
	}

	public delegate void ActivatableStateChangedHandler(IActivatable activatable, ActivatableState state);

	public interface IActivatable
	{
		/// <summary>
		/// Текущее состояние активируемого объекта.
		/// </summary>
		ActivatableState ActivatableState { get; }

		/// <summary>
		/// Событие изменения текущего состояния.
		/// </summary>
		event ActivatableStateChangedHandler ActivatableStateChangedEvent;

		/// <summary>
		/// Активировать объект.
		/// </summary>
		/// <param name="immediately">Флаг, указывающий активировать объект немедленно.</param>
		void Activate(bool immediately = false);

		/// <summary>
		/// Деактивировать объект.
		/// </summary>
		/// <param name="immediately">Флаг, указывающий деактивировать объект немедленно.</param>
		void Deactivate(bool immediately = false);
	}
}