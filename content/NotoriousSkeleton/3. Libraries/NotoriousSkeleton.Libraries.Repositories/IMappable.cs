namespace NotoriousSkeleton.Libraries.Repositories;
public interface IMappable<TModel, TEntity>
{
    TModel ToModel();
    static abstract TEntity FromModel(TModel model);
}

public interface IMappable<TModel>
{
    TModel ToModel();
}
