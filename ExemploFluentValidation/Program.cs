using FluentValidation;
using FluentValidation.Results;
using System;

namespace ExemploFluentValidation
{
    class Program
    {
        static void Main(string[] args)
        {
            var p = new Pessoa(nome: null,telefone: null,email: null,cpf: null);
            var reultado = p.Validar();
            if (!reultado.IsValid)
            {
                foreach (var mensagem in reultado.Errors)
                {
                    Console.WriteLine(mensagem);
                }
            }
            

        }
    }

    public class Pessoa
    {

        public string Nome { get; private set; }
        public string Telefone { get; private set; }
        public string Email { get; private set; }
        public string Cpf { get; private set; }

        readonly PessoaValidator validator = new PessoaValidator();

        public Pessoa(string nome, string telefone, string email, string cpf)
        {
            Nome = nome;
            Telefone = telefone;
            Email = email;
            Cpf = cpf;
            
        }

        public ValidationResult Validar()
        {
            return validator.Validate(this);
            
        }
    }

    public class PessoaValidator : AbstractValidator<Pessoa>
    {
        public PessoaValidator()
        {
            RuleFor(x => x.Nome)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(20);

            RuleFor(x => x.Telefone)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .NotEmpty()
                .Matches(@"^\d{8}|d{9}$");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Cpf)
                .CpfValido();
        }
    }

    public static class CpfValidation
    {

        public static IRuleBuilderInitial<T, string> CpfValido<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Custom((cpf, context) =>
            {
                //Impirado na validação de CPF proposta por ElemarJr.
                //https://www.eximiaco.ms/pt/2020/01/10/no-c-8-ficou-mais-facil-alocar-arrays-na-stack-e-isso-pode-ter-um-impacto-positivo-tremendo-na-performance/

                if (string.IsNullOrWhiteSpace(cpf))
                {
                    context.AddFailure($"'{context.DisplayName}' não pode ser nulo ou vazio.");
                    return;
                }


                Span<int> cpfArray = stackalloc int[11];
                var count = 0;
                foreach (var c in cpf)
                {
                    if (!char.IsDigit(c))
                    {
                        context.AddFailure($"'{context.DisplayName}' tem que ser numérico.");
                        return;
                    }


                    if (char.IsDigit(c))
                    {
                        if (count > 10)
                        {
                            context.AddFailure($"'{context.DisplayName}' deve possuir 11 caracteres. Foram informados " + cpf.Length);
                            return;
                        }


                        cpfArray[count] = c - '0';
                        count++;
                    }
                }

                if (count != 11)
                {
                    context.AddFailure($"'{context.DisplayName}' deve possuir 11 caracteres. Foram informados " + cpf.Length);
                    return;
                }
                if (VerificarTodosValoresSaoIguais(ref cpfArray))
                {
                    context.AddFailure($"'{context.DisplayName}' Não pode conter todos os dígitos iguais.");
                    return;
                }

                var totalDigitoI = 0;
                var totalDigitoII = 0;
                int modI;
                int modII;

                for (var posicao = 0; posicao < cpfArray.Length - 2; posicao++)
                {
                    totalDigitoI += cpfArray[posicao] * (10 - posicao);
                    totalDigitoII += cpfArray[posicao] * (11 - posicao);
                }

                modI = totalDigitoI % 11;
                if (modI < 2) { modI = 0; }
                else { modI = 11 - modI; }

                if (cpfArray[9] != modI)
                {
                    context.AddFailure($"'{context.DisplayName}' Inválido.");
                    return;
                }

                totalDigitoII += modI * 2;

                modII = totalDigitoII % 11;
                if (modII < 2) { modII = 0; }
                else { modII = 11 - modII; }

                return;

            });
        }

        static bool VerificarTodosValoresSaoIguais(ref Span<int> input)
        {
            for (var i = 1; i < 11; i++)
            {
                if (input[i] != input[0])
                {
                    return false;
                }
            }

            return true;
        }
    }

}
