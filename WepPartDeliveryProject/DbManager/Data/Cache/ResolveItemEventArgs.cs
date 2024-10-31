
namespace DbManager.Data.Cache
{
    public class ResolveItemEventArgs<TItem, T> : EventArgs
            where T : IComparable
            where TItem : IModel
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id">id, по ктором надо произвести поиск</param>
        public ResolveItemEventArgs(T id)
        {
            this._id = id;
        }

        readonly T _id;
        /// <summary>
        /// Id для поиска
        /// </summary>
        public T ItemId { get => this._id; }

        TItem _item;
        /// <summary>
        /// Найденный объект
        /// </summary>
        public TItem ResolvedItem
        {
            get => this._item;
            set => this._item = value;
        }
    }
}
