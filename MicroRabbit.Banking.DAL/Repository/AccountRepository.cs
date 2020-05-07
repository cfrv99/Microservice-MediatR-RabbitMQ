using MicroRabbit.Banking.DAL.Context;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Banking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Banking.DAL.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly BankingDbContext context;

        public AccountRepository(BankingDbContext context)
        {
            this.context = context;
        }

        public void Add(Account entity)
        {
            context.Accounts.Add(entity);
        }

        public IEnumerable<Account> GetAccounts()
        {
            return context.Accounts;
        }

    }
}
