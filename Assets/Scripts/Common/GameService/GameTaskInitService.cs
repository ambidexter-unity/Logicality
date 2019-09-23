using System;
using Common.GameTask;
using UnityEngine;
using UnityEngine.Assertions;

namespace Common.GameService
{
	/// <summary>
	/// Вспомогательная задача на инициализацию сервиса.
	/// </summary>
	public class GameTaskInitService : IGameTask, IDisposable
	{
		private bool _completed;
		private readonly IGameService _gameService;

		private bool _isDisposed;
		private bool _isStarted;

		public GameTaskInitService(IGameService gameService)
		{
			_gameService = gameService;
		}

		// ITask

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

		public void Start()
		{
			if (_isStarted || _isDisposed) return;
			_isStarted = true;

			if (_gameService.IsReady)
			{
				Debug.LogError("The Service is ready when the Task starts.");
				Completed = true;
				return;
			}

			_gameService.ReadyEvent += OnServiceReady;
			_gameService.Initialize();
		}

		// \ITask

		// IDisposable

		public void Dispose()
		{
			if (_isDisposed) return;
			_isDisposed = true;

			_gameService.ReadyEvent -= OnServiceReady;
		}

		// \IDisposable

		private void OnServiceReady(IGameService service)
		{
			_gameService.ReadyEvent -= OnServiceReady;
			Completed = true;
		}
	}
}