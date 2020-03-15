﻿// Copyright (C)  - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Maksim Kolesnik maksim.kolesnik@emka3.de, 2019
using System.Collections.Generic;
using System.Threading.Tasks;
using Emka3.PracticeLooper.Model.Common;

namespace Emka3.PracticeLooper.Services.Contracts.Common
{
    public interface IRepository<T> where T : EntityBase
    {
        void Delete(T item);
        void Save(T item);
        T GetById(int id);
        List<T> GetAllItems();
        Task DeleteAsync(T item);
        Task SaveAsync(T item);
        Task<T> GetByIdAsync(int id);
        Task<List<T>> GetAllItemsAsync(); 
    }
}
