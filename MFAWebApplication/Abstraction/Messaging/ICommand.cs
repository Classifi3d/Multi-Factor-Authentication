using CSharpFunctionalExtensions;

namespace MFAWebApplication.Abstraction.Messaging;
public interface ICommand : IBaseCommand
{
}

public interface ICommand<TReponse> : IBaseCommand
{

}

public interface IBaseCommand
{
     
}
