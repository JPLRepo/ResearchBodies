using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;
using Strategies;
using ContractConfigurator;
using ContractConfigurator.ExpressionParser;

namespace ResearchBodies
{
    public class ContractExpressionParser : ClassExpressionParser<StrategiaStrategy>, IExpressionParserRegistrer
    {
        static ContractExpressionParser()
        {
            RegisterMethods();
        }

        public void RegisterExpressionParsers()
        {
            RegisterParserType(typeof(StrategiaStrategy), typeof(ContractExpressionParser));
        }

        internal static void RegisterMethods()
        {
            RegisterMethod(new Method<StrategiaStrategy, string>("Name", s => s != null ? s.Config.Title : ""));
            RegisterMethod(new Method<StrategiaStrategy, string>("Type", s => s != null ? s.GetType().BaseType.Name : ""));
            RegisterMethod(new Method<StrategiaStrategy, string>("contractType", s => ContractEffectField(s, ce => ce.contractType)));

            RegisterMethod(new Method<StrategiaStrategy, CelestialBody>("targetBody", s => ContractEffectField(s, ce => ce.targetBody)));
            RegisterMethod(new Method<StrategiaStrategy, List<CelestialBody>>("bodies", s => ContractEffectField(s, ce => ce.bodies)));
            RegisterMethod(new Method<StrategiaStrategy, string>("description", s => s != null ? s.Description : ""));
            RegisterMethod(new Method<StrategiaStrategy, string>("synopsis", s => ContractEffectField(s, ce => ce.synopsis)));
            RegisterMethod(new Method<StrategiaStrategy, string>("completedMessage", s => ContractEffectField(s, ce => ce.completedMessage)));

            RegisterMethod(new Method<StrategiaStrategy, double>("advanceFunds", s => ContractEffectField(s, ce => ce.advanceFunds)));
            RegisterMethod(new Method<StrategiaStrategy, double>("rewardFunds", s => ContractEffectField(s, ce => ce.rewardFunds)));
            RegisterMethod(new Method<StrategiaStrategy, float>("rewardScience", s => ContractEffectField(s, ce => ce.rewardScience)));
            RegisterMethod(new Method<StrategiaStrategy, float>("rewardReputation", s => ContractEffectField(s, ce => ce.rewardReputation)));
            RegisterMethod(new Method<StrategiaStrategy, double>("failureFunds", s => ContractEffectField(s, ce => ce.failureFunds)));
            RegisterMethod(new Method<StrategiaStrategy, float>("failureReputation", s => ContractEffectField(s, ce => ce.failureReputation)));

            RegisterGlobalFunction(new Function<List<StrategiaStrategy>>("ActiveStrategies", () => StrategySystem.Instance != null ?
                StrategySystem.Instance.Strategies.OfType<StrategiaStrategy>().Where(s => s.IsActive).ToList() : new List<StrategiaStrategy>(), false));
        }

        public ContractExpressionParser()
        {
        }

        private static T ContractEffectField<T>(StrategiaStrategy strategy, Func<ContractEffect, T> func)
        {
            if (strategy == null)
            {
                return default(T);
            }

            ContractEffect contractEffect = strategy.Effects.OfType<ContractEffect>().FirstOrDefault();
            if (contractEffect == null)
            {
                return default(T);
            }

            return func.Invoke(contractEffect);
        }

        public override bool ConvertableFrom(Type type)
        {
            return typeof(StrategiaStrategy).IsAssignableFrom(type);
        }

        public override StrategiaStrategy ConvertFrom<U>(U value)
        {
            return (StrategiaStrategy)(object)value;
        }

        public override U ConvertType<U>(StrategiaStrategy value)
        {
            if (typeof(U) == typeof(string))
            {
                return (U)(object)value.Title;
            }
            return base.ConvertType<U>(value);
        }
    }
}
