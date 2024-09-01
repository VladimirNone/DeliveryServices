using Bogus;

namespace DbManager.Neo4j.DataGenerator
{
    internal static class BogusExtentions
    {
        /// <summary>
        /// Get a random list item and remove it from list.
        /// </summary>
        public static T ListItemWithRemove<T>(this Randomizer randomizer, List<T> list)
        {
            var item = randomizer.ListItem(list as IList<T>);
            list.Remove(item);
            return item;
        }
    }
}
