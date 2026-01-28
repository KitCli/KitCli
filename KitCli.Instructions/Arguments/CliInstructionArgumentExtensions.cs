using KitCli.Instructions.Abstractions;

namespace KitCli.Instructions.Arguments;

public static class CliInstructionArgumentExtensions
{
    public static ValuedCliInstructionArgument<TArgumentType>? OfType<TArgumentType>(
        this IEnumerable<CliInstructionArgument> arguments, string argumentName)
        where TArgumentType : notnull
            => arguments
                .Where(argument => argument.ArgumentName == argumentName)
                .OfType<ValuedCliInstructionArgument<TArgumentType>>()
                .FirstOrDefault();
    
    public static ValuedCliInstructionArgument<TArgumentType> OfRequiredType<TArgumentType>(
        this IEnumerable<CliInstructionArgument> arguments, string argumentName)
        where TArgumentType : notnull
    {
        var argument = OfType<TArgumentType>(arguments, argumentName);

        if (argument is null)
        {
            throw new CliInstructionException(CliInstructionExceptionCode.ArgumentIsRequired,
                $"Argument '{argumentName}' is required for this command.");
        }
        
        return argument;
    }

    public static ValuedCliInstructionArgument<string> OfRequiredStringFrom<TPossibleArgumentType>(
        this IEnumerable<CliInstructionArgument> arguments, string argumentName)
        where TPossibleArgumentType : notnull
    {
        var possibleArgument = OfRequiredType<TPossibleArgumentType>(arguments, argumentName);
        var stringifiedArgumentValue = possibleArgument.ArgumentValue.ToString();
        return new ValuedCliInstructionArgument<string>(argumentName, stringifiedArgumentValue!);
    }

    // Works around the constraint that argument passing has to be one or the other, but we can 
    // work out which one it is.
    public static ValuedCliInstructionArgument<string> OfRequiredStringFrom<TPossibleArgumentTypeOne, TPossibleArgumentTypeTwo>(
        this List<CliInstructionArgument> arguments, string argumentName) 
        where TPossibleArgumentTypeOne : notnull
        where TPossibleArgumentTypeTwo : notnull
    {
        var possibleArgumentOne = OfType<TPossibleArgumentTypeOne>(arguments, argumentName);
        if (possibleArgumentOne is not null)
        {
            var stringifiedArgumentValue = possibleArgumentOne.ArgumentValue.ToString();
            return new ValuedCliInstructionArgument<string>(argumentName, stringifiedArgumentValue!);
        }
        
        var possibleArgumentTwo = OfType<TPossibleArgumentTypeTwo>(arguments, argumentName);
        if (possibleArgumentTwo is not null)
        {
            var stringifiedArgumentValue = possibleArgumentTwo.ArgumentValue.ToString();
            return new ValuedCliInstructionArgument<string>(argumentName, stringifiedArgumentValue!);
        }
        
        throw new CliInstructionException(CliInstructionExceptionCode.ArgumentIsRequired,
                $"Argument '{argumentName}' is required for this command.");
    }
}