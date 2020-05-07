using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Domain.Interfaces;
using MicroRabbit.Transfer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Transfer.Data.Repository
{
    public class TransferRepository : ITransferRepository
    {
        private readonly TransferDbContext context;

        public TransferRepository(TransferDbContext context)
        {
            this.context = context;
        }
        public IEnumerable<TransferLog> GetTransferLogs()
        {
            return context.TransferLogs;
        }
    }
}
