﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using mRemoteNG.Tools;

namespace mRemoteNG.Credential.Repositories
{
    public class CompositeRepositoryUnlocker
    {
        private readonly List<ICredentialRepository> _repositories = new List<ICredentialRepository>();

        public IEnumerable<ICredentialRepository> Repositories => _repositories;
        public Optional<ICredentialRepository> SelectedRepository { get; set; } = Optional<ICredentialRepository>.Empty;

        public CompositeRepositoryUnlocker(IEnumerable<ICredentialRepository> repositories)
        {
            if (repositories == null)
                throw new ArgumentNullException(nameof(repositories));

            _repositories.AddRange(repositories);
            SelectNextLockedRepository();
        }

        public void Unlock(SecureString key)
        {
            if (SelectedRepository.Any())
                SelectedRepository.First().LoadCredentials(key);
        }

        public void SelectNextLockedRepository()
        {
            SelectedRepository = GetNextLockedRepo();
        }

        private Optional<ICredentialRepository> GetNextLockedRepo()
        {
            var newOrder = OrderListForNextLockedRepo();
            return new Optional<ICredentialRepository>(newOrder.FirstOrDefault());
        }

        private IList<ICredentialRepository> OrderListForNextLockedRepo()
        {
            if (_repositories.Count == 0)
                return new List<ICredentialRepository>();
            var reorderedList = new List<ICredentialRepository>();
            var itemsAfterCurrent = BuildListOfItemsAfterCurrent();
            var itemsBeforeAndIncludingCurrent = BuildListOfItemsBeforeAndIncludingCurrent();
            reorderedList.AddRange(itemsAfterCurrent.Where(repository => !repository.IsLoaded));
            reorderedList.AddRange(itemsBeforeAndIncludingCurrent.Where(repository => !repository.IsLoaded));
            return reorderedList;
        }

        private IList<ICredentialRepository> BuildListOfItemsAfterCurrent()
        {
            var lastListIndex = _repositories.Count - 1;
            var newListStartIndex = GetNewListStartIndex();

            if (newListStartIndex > lastListIndex) newListStartIndex--;
            var countToEndOfList = _repositories.Count - newListStartIndex;
            return _repositories.GetRange(newListStartIndex, countToEndOfList);
        }

        private IList<ICredentialRepository> BuildListOfItemsBeforeAndIncludingCurrent()
        {
            var newListStartIndex = GetNewListStartIndex();
            return _repositories.GetRange(0, newListStartIndex);
        }

        private int GetNewListStartIndex()
        {
            if (!SelectedRepository.Any())
                return 0;

            var currentItemIndex = _repositories.IndexOf(SelectedRepository.First());
            return currentItemIndex + 1;
        }
    }
}