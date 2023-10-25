// My very bad code
using System.Collections.Generic;
using System.Data;

namespace MethodInjectorOld;
public delegate TReturn ExternalMethodDelegate<TClass, TParam, TReturn>(TClass obj, TParam param, BaseMethod<TParam, TReturn> baseCaller);
public delegate TReturn BaseMethod<TParam, TReturn>(TParam param);

class MethodCaller<TClass, TParam, TReturn>
{
    IEnumerator<ImplementationTree<TClass, TParam, TReturn>> MethodCallOrder;
    public MethodCaller(IEnumerable<ImplementationTree<TClass, TParam, TReturn>> methodCallOrder)
    {
        MethodCallOrder = methodCallOrder.GetEnumerator();
    }
    public TReturn CallNext(TClass obj, TParam param)
    {
        MethodCallOrder.MoveNext();
        return MethodCallOrder.Current.CurrentImplementation.Invoke(
            obj,
            param,
            newparam => CallNext(obj, newparam)
        );
    }
}
abstract class ImplementationTree<TParam, TReturn>
{
    public abstract bool CanCastToCurrentClass(object obj);
    public abstract IEnumerable<ImplementationTree<tc, TParam, TReturn>> FindFunc<tc>(tc obj) where tc : class;
}
class ImplementationTree<TClass, TParam, TReturn> : ImplementationTree<TParam, TReturn>
{
    public ImplementationTree(ExternalMethodDelegate<TClass, TParam, TReturn> currentImplementation)
    {
        CurrentImplementation = currentImplementation;
    }

    public override bool CanCastToCurrentClass(object obj) => obj is TClass;
    public List<ImplementationTree<TParam, TReturn>> Children { get; } = new();
    public ExternalMethodDelegate<TClass, TParam, TReturn> CurrentImplementation { get; set; }
    public override IEnumerable<ImplementationTree<tc, TParam, TReturn>> FindFunc<tc>(tc obj)
    {
        if (IsAssignableFrom<tc, TClass>())
        {
            return (IEnumerable<ImplementationTree<tc, TParam, TReturn>>)(object)FindFunc((TClass)(object)obj);
        }
    }

    public IEnumerable<ImplementationTree<TClass, TParam, TReturn>> FindFunc(TClass obj)
    {
        foreach (var child in Children)
        {
            if (child.CanCastToCurrentClass(obj!))
            {
                bool atLeastOne = false;
                foreach (var ret in child.FindFunc(obj))
                {
                    atLeastOne = true;
                    yield return ret;
                }
                if (atLeastOne)
                {
                    yield return this;
                    yield break;
                }
                // else, look at other branch
            }
        }
        yield return this;
    }
    public TReturn Call<TSubClass>(TSubClass obj, TParam param) where TSubClass : TClass
        => new MethodCaller<TClass, TParam, TReturn>(FindFunc(obj)).CallNext(obj, param);
    public void Register<TSubClass>(ExternalMethodDelegate<TSubClass, TParam, TReturn> func) where TSubClass : TClass
    {
        if (IsSame<TSubClass, TClass>())
        {
            throw new System.InvalidOperationException($"The method is already implemented for type {typeof(TSubClass).FullName}");
        }
        // Let the children handle
        foreach (var child in Children)
        {
            if (child.IsValidChildType<TSubClass>())
            {
                child.Register(func);
                return;
            }
        }
        // If there is no one accepting the method, we add one ourselves
        var subclassimpl = new ImplementationTree<TSubClass, TParam, TReturn>(func);
        Children.Add((ImplementationTree<TClass, TParam, TReturn>)subclassimpl);
    }
    bool IsValidChildType<T>() => IsAssignableFrom<T, TClass>();
    static bool IsAssignableFrom<TPotentialSubclass, TBaseClass>() => 
        typeof(TBaseClass).IsAssignableFrom(typeof(TPotentialSubclass));
    static bool IsSame<TClass2, TClass1>() =>
        typeof(TClass2) == typeof(TClass1);
}
public class ExternalMethodService<TClass, TParam, TReturn>
{
    ImplementationTree<TClass, TParam, TReturn> root;
    public ExternalMethodService(ExternalMethodDelegate<TClass, TParam, TReturn> baseImplementation)
    {
        root = new(baseImplementation);
    }
    public ExternalMethodService() : this(
        (_, _, _) =>
            throw new System.NotImplementedException(
                "The base method is not implemented and implementation of child class cannot be found."
            )
        )
    {

    }
    public TReturn Call<TSubClass>(TSubClass obj, TParam param) where TSubClass : TClass
        => root.Call(obj, param);
    public void Register<TSubClass>(ExternalMethodDelegate<TSubClass, TParam, TReturn> func) where TSubClass : TClass
    {

    }
}