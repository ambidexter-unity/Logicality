using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace Common.GameTask
{
	/// <inheritdoc cref="IGameTask" />
	/// <summary>
	/// Параллельное выполнение задач.
	/// </summary>
	public class GameTaskConcurent : IGameTask, IDisposable
	{
		private bool _completed;
		private bool _isStarted;
		private int _startedTasksCount;
		private readonly List<IGameTask> _tasks = new List<IGameTask>();
		private readonly Mutex _completeMutex = new Mutex();

		private bool _isDisposed;

		// ITask

		public void Start()
		{
			if (_isStarted || Completed || _isDisposed) return;

			_isStarted = true;
			_startedTasksCount = _tasks.Count;
			if (_startedTasksCount > 0)
			{
				foreach (var task in _tasks.ToArray())
				{
					if (task.Completed)
					{
						Debug.LogWarning("Task in concurent already completed.");
						_tasks.Remove(task);
						return;
					}

					task.CompleteEvent += SubTaskCompleteHandler;
				}

				_tasks.ForEach(task => task.Start());
			}
			else
			{
				Completed = true;
			}
		}

		public bool Completed
		{
			get => _completed;
			private set
			{
				if (value == _completed) return;

				Assert.IsFalse(_completed);
				_completed = value;
				CompleteEvent?.Invoke(this);
			}
		}

		public event GameTaskCompleteHandler CompleteEvent;

		// \ITask

		// IDisposable

		public void Dispose()
		{
			if (_isDisposed) return;
			_isDisposed = true;

			if (_isStarted)
			{
				_tasks.ForEach(task => task.CompleteEvent -= SubTaskCompleteHandler);
			}

			_tasks.ForEach(task => (task as IDisposable)?.Dispose());
			_tasks.Clear();

			if (CompleteEvent != null)
			{
				// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
				foreach (GameTaskCompleteHandler handler in CompleteEvent.GetInvocationList())
					CompleteEvent -= handler;
			}
		}

		// \IDisposable

		/// <summary>
		/// Очистить параллельное выполнение.
		/// </summary>
		public void Clear()
		{
			if (_isDisposed) return;

			if (_isStarted)
			{
				_tasks.ForEach(task => task.CompleteEvent -= SubTaskCompleteHandler);
			}

			_tasks.Clear();
		}

		/// <summary>
		/// Добавить задачу в параллельное выполнение.
		/// </summary>
		/// <param name="gameTask">Добавляемая задача.</param>
		/// <exception cref="Exception">Параллельное выполнение уже запущено.</exception>
		public void Add(IGameTask gameTask)
		{
			if (_isDisposed) return;

			if (_isStarted) throw new Exception("Concurent already executed.");
			Assert.IsFalse(Completed, "Concurent already completed, added task will have no effect.");
			_tasks.Add(gameTask);
		}

		private void SubTaskCompleteHandler(IGameTask task)
		{
			task.CompleteEvent -= SubTaskCompleteHandler;
			_tasks.Remove(task);

			var completed = false;
			if (_completeMutex.WaitOne())
			{
				--_startedTasksCount;
				completed = _startedTasksCount <= 0;
				_completeMutex.ReleaseMutex();
			}

			if (completed) Completed = true;
		}
	}
}