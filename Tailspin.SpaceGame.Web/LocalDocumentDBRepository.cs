using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TailSpin.SpaceGame.Web.Models;

namespace TailSpin.SpaceGame.Web
{
    public class LocalDocumentDBRepository : IDocumentDBRepository
    {
        // An in-memory list of all items in the collection.
        private readonly List<Score> _scores;
        private readonly List<Profile> _profiles;

        public LocalDocumentDBRepository(string scoresFileName, string profilesFileName)
        {
            // Serialize the items from the provided JSON document.
            _scores = JsonConvert.DeserializeObject<List<Score>>(File.ReadAllText(scoresFileName));
            _profiles = JsonConvert.DeserializeObject<List<Profile>>(File.ReadAllText(profilesFileName));
        }

        /// <summary>
        /// Retrieves the item from the store with the given identifier.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the retrieved item.
        /// </returns>
        /// <param name="id">The identifier of the item to retrieve.</param>
        public Task<Profile> GetProfileAsync(string profileId)
        {
            return Task<Profile>.FromResult(_profiles.Single(profile => profile.Id == profileId));
        }

        /// <summary>
        /// Retrieves items from the store that match the given query predicate.
        /// Results are given in descending order by the given ordering predicate.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the collection of retrieved items.
        /// </returns>
        /// <param name="queryPredicate">Predicate that specifies which items to select.</param>
        /// <param name="orderDescendingPredicate">Predicate that specifies how to sort the results in descending order.</param>
        /// <param name="page">The 1-based page of results to return.</param>
        /// <param name="pageSize">The number of items on a page.</param>
        public Task<IEnumerable<Score>> GetScoresAsync(
            string mode,
            string region,
            int page = 1, int pageSize = 10
        )
        {
            Expression<Func<Score, bool>> queryPredicate = score =>
                            (string.IsNullOrEmpty(mode) || score.GameMode == mode) &&
                            (string.IsNullOrEmpty(region) || score.GameRegion == region);

            var result = _scores.AsQueryable()
                .Where(queryPredicate) // filter
                .OrderByDescending(score => score.HighScore) // sort
                .Skip((page - 1) * pageSize) // find page
                .Take(pageSize) // take items
                .AsEnumerable(); // make enumerable

            return Task<IEnumerable<Score>>.FromResult(result);
        }

        /// <summary>
        /// Retrieves the number of items that match the given query predicate.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result contains the number of items that match the query predicate.
        /// </returns>
        /// <param name="queryPredicate">Predicate that specifies which items to select.</param>
        public Task<int> CountScoresAsync(string mode, string region)
        {
            Expression<Func<Score, bool>> queryPredicate = score =>
                (string.IsNullOrEmpty(mode) || score.GameMode == mode) &&
                (string.IsNullOrEmpty(region) || score.GameRegion == region);

            var count = _scores.AsQueryable()
                .Where(queryPredicate) // filter
                .Count(); // count

            return Task<int>.FromResult(count);
        }
    }
}