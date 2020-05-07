using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Transfer.Application.Interfaces;
using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Domain.Interfaces;
using MicroRabbit.Transfer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Transfer.Application.Services
{
    public class TransferService : ITransferService
    {
        private readonly ITransferRepository transferRepository;

        public TransferService(ITransferRepository transferRepository,
            IEventBus)
        {
            this.transferRepository = transferRepository;
        }
        public IEnumerable<TransferLog> GetTransferLogs()
        {
            return transferRepository.GetTransferLogs();
        }
    }
}
