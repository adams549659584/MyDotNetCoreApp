using System;

namespace My.App.Core
{
    public interface IMongoEnt
    {
        /// <summary>
        /// ID
        /// </summary>
        Guid Id { get; set; }
    }
}