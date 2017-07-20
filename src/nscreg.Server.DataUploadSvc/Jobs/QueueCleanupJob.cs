﻿using System;
using System.Threading;
using nscreg.Data;
using nscreg.Server.DataUploadSvc.Interfaces;
using nscreg.Services.DataSources;

namespace nscreg.Server.DataUploadSvc.Jobs
{
    public class QueueCleanupJob : IJob
    {
        public int Interval { get; }

        private readonly int _timeout;
        private readonly QueueService _queueSvc;

        public QueueCleanupJob(NSCRegDbContext ctx, int dequeueInterval, int timeout)
        {
            Interval = dequeueInterval;
            _queueSvc = new QueueService(ctx);
            _timeout = timeout;
        }

        public async void Execute(CancellationToken cancellationToken)
        {
            await _queueSvc.ResetDequeuedByTimeout(_timeout);
        }

        public void OnException(Exception e)
        {
            throw new NotImplementedException();
        }
    }
}