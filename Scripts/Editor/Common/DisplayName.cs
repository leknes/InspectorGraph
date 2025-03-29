using UnityEngine;
using System;
using System.Text;

namespace InspectorGraph
{
    public static class DisplayName
    {
        public static string PropertyName(string name)
        {
            int length = name.Length;

            var builder = new StringBuilder(length);

            int index = 1;

            if (length == 1 || name[index] != '_')
                index = 0;

            while (index < length)
            {
                if (name[index] != '_')
                    break;

                index++;
            }

            if (index == length)
                return name;

            builder.Append(char.ToUpper(name[index]));

            bool prepreviousCase = false;

            index++;

            if (index == length)
                return builder.ToString();

            char previousChar = name[index];
            bool previousCase = char.IsLower(previousChar);

            for (int i = index + 1; i < length; i++)
            {
                char currentChar = name[i];
                bool currentCase = char.IsLower(currentChar);

                if (previousCase != currentCase)
                {
                    switch (previousCase, currentCase)
                    {
                        case (false, true):
                            if (prepreviousCase == false)
                                builder.Append(' ');
                            builder.Append(previousChar);
                            break;

                        case (_, true):
                            builder.Append(previousChar);
                            builder.Append(' ');
                            currentChar = char.ToUpper(currentChar);
                            currentCase = false;
                            break;

                        default:
                            builder.Append(previousChar);
                            builder.Append(' ');
                            break;
                    }
                }
                else
                    builder.Append(previousChar);

                prepreviousCase = previousCase;

                previousChar = currentChar;
                previousCase = currentCase;
            }

            builder.Append(previousChar);

            return builder.ToString();
        }

        public static string TypeName(Type type)
        {
            string name = type.Name;

            int index = name.LastIndexOf('`');

            if (index != -1)
                name = name.Remove(index);

            name = PropertyName(name);

            var typeArguments = type.GenericTypeArguments;

            if (typeArguments.Length == 0)
                return name;

            string[] typeArgumentNames = new string[typeArguments.Length];

            for (int i = 0; i < typeArguments.Length; i++)
            {
                typeArgumentNames[i] = TypeName(typeArguments[i]);
            }

            return $"{name} {{{string.Join(" ,", typeArgumentNames)}}}";
        }
    }
}