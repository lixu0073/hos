using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;
using System;

namespace Transactions
{

    public abstract class BaseTransactionController<T> where T : ITransaction<T>
    {

        private const string TAG = "TRANSACTIONS";

        protected Dictionary<string, string> unparsedTransactions = new Dictionary<string, string>();

        public abstract void Save(Save save);
        public abstract void Load(Save save);
        public abstract void ResendAction(string key, string unparsedModel);

        protected List<string> MapTransactions(Dictionary<string, string> transactionsDictionary)
        {
            List<string> transactionsList = new List<string>();
            if (transactionsDictionary == null)
                return transactionsList;
            foreach (KeyValuePair<string, string> pair in transactionsDictionary)
            {
                transactionsList.Add(pair.Key + "*" + pair.Value);
            }
            return transactionsList;
        }

        protected Dictionary<string, string> MapTransactions(List<string> transactionsList)
        {
            Dictionary<string, string> transactionsDictionary = new Dictionary<string, string>();
            if (transactionsList == null)
                return transactionsDictionary;
            foreach(string data in transactionsList)
            {
                string[] array = data.Split('*');
                transactionsDictionary.Add(array[0], array[1]);
            }
            return transactionsDictionary;
        }

        public bool IsTransactionExist(ITransaction<T> transaction)
        {
            return unparsedTransactions.ContainsKey(transaction.GetKey());
        }

        public bool CompleteTransaction(string key)
        {
            bool removeSuccess = unparsedTransactions.Remove(key);
            if(removeSuccess)
            {
#if UNITY_EDITOR
                Debug.Log(TAG + " : [" + GetType() + "] : CompleteTransaction : " + key);
#endif
            }
            return removeSuccess;
        }

        public bool CompleteTransaction(ITransaction<T> transaction)
        {
            if(IsTransactionExist(transaction))
            {
#if UNITY_EDITOR
                Debug.Log(TAG + " : [" + GetType() + "] : CompleteTransaction : " + transaction.GetKey());
#endif
                unparsedTransactions.Remove(transaction.GetKey());
                return true;
            }
            return false;
        }

        public bool AddTransaction(ITransaction<T> transaction)
        {
            if (IsTransactionExist(transaction))
                return false;
#if UNITY_EDITOR
            Debug.Log(TAG + " : [" + GetType() + "] : AddTransaction : " + transaction.GetKey());
#endif
            unparsedTransactions.Add(transaction.GetKey(), transaction.Stringify());
            return true;
        }

        private void ResendActionInternal(string key, string unparsedModel)
        {
#if UNITY_EDITOR
            Debug.Log(TAG + " : [" + GetType() + "] : ResendActionInternal : " + key);
#endif
            ResendAction(key, unparsedModel);
        }

        public void CheckingForResendActions(List<ITransaction<T>> transactions, bool IsInsertMode = true)
        {
            List<ITransaction<T>> transactionsToComplete = new List<ITransaction<T>>();
            List<string> transactionsToCompleteKeys = new List<string>();
            foreach (KeyValuePair<string, string> pair in unparsedTransactions)
            {
                bool DoBody = false;
                for(int i=0; i<transactions.Count; ++i)
                {
                    if(pair.Key == transactions[i].GetKey())
                    {
                        if (IsInsertMode)
                            transactionsToComplete.Add(transactions[i]);
                        else
                            ResendActionInternal(pair.Key, pair.Value);
                        DoBody = true;
                        break;
                    }
                }
                if (!DoBody)
                {
                    if (IsInsertMode)
                        ResendActionInternal(pair.Key, pair.Value);
                    else
                        transactionsToCompleteKeys.Add(pair.Key);
                }
            }
            foreach(ITransaction<T> transaction in transactionsToComplete)
                CompleteTransaction(transaction);
            foreach(string key in transactionsToCompleteKeys)
                CompleteTransaction(key);

        }

    }
}
