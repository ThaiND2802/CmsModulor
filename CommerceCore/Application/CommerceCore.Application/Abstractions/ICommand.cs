using CommerceCore.SharedKernel.Primitives;

namespace CommerceCore.Application.Abstractions;

public interface ICommand;

public interface ICommand<out TResult> where TResult : Result;
