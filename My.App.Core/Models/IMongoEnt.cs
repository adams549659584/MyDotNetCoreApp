using System;

namespace My.App.Job
{
    public interface IMongoEnt
    {
        /// <summary>
        /// ID
        /// </summary>
        Guid Id { get; set; }
    }
}