﻿namespace LinqToDB.Data
{
	/// <summary>
	/// Data connection transaction controller.
	/// </summary>
	public class DataConnectionTransaction : IDisposable,
#if NATIVE_ASYNC
		IAsyncDisposable
#else
		Async.IAsyncDisposable
#endif
	{
		/// <summary>
		/// Creates new transaction controller for data connection.
		/// </summary>
		/// <param name="dataConnection">Data connection instance.</param>
		public DataConnectionTransaction(DataConnection dataConnection)
		{
			DataConnection = dataConnection ?? ThrowHelper.ThrowArgumentNullException<DataConnection>(nameof(dataConnection));
		}

		/// <summary>
		/// Returns associated data connection instance.
		/// </summary>
		public DataConnection DataConnection { get; }

		bool _disposeTransaction = true;

		/// <summary>
		/// Commits current transaction for data connection.
		/// </summary>
		public void Commit()
		{
			DataConnection.CommitTransaction();
			_disposeTransaction = false;
		}

		/// <summary>
		/// Rolllbacks current transaction for data connection.
		/// </summary>
		public void Rollback()
		{
			DataConnection.RollbackTransaction();
			_disposeTransaction = false;
		}

		/// <summary>
		/// Commits current transaction for data connection asynchonously.
		/// If underlying provider doesn't support asynchonous commit, it will be performed synchonously.
		/// </summary>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Asynchronous operation completion task.</returns>
		public async Task CommitAsync(CancellationToken cancellationToken = default)
		{
			await DataConnection.CommitTransactionAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			_disposeTransaction = false;
		}

		/// <summary>
		/// Rollbacks current transaction for data connection asynchonously.
		/// If underlying provider doesn't support asynchonous rollback, it will be performed synchonously.
		/// </summary>
		/// <param name="cancellationToken">Asynchronous operation cancellation token.</param>
		/// <returns>Asynchronous operation completion task.</returns>
		public async Task RollbackAsync(CancellationToken cancellationToken = default)
		{
			await DataConnection.RollbackTransactionAsync(cancellationToken).ConfigureAwait(Common.Configuration.ContinueOnCapturedContext);
			_disposeTransaction = false;
		}

		public void Dispose()
		{
			if (_disposeTransaction)
				DataConnection.RollbackTransaction();
		}

#if NATIVE_ASYNC
		public ValueTask DisposeAsync()
		{
			if (_disposeTransaction)
				return new ValueTask(DataConnection.RollbackTransactionAsync());

			return default;
		}
#else
		public Task DisposeAsync()
		{
			if (_disposeTransaction)
				return DataConnection.RollbackTransactionAsync();

			return TaskEx.CompletedTask;
		}
#endif
	}
}
