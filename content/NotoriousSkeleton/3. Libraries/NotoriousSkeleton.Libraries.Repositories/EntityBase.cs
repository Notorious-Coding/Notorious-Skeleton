namespace NotoriousSkeleton.Libraries.Repositories;
public class EntityBase<TId>
{
    public TId Id { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime UpdateDate { get; set; }
}
