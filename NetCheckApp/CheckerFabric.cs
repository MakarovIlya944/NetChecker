using System;
using System.Collections.Generic;
using System.Text;

namespace Core.NetChecker {
    public enum CheckerMode {
        КомпонентыСвязности,
        Объем,
        МКЭ
    };

    interface ICheckerFabric {
        INetChecker Create(CheckerMode type);
    }

    public class CheckerFabric : ICheckerFabric {
        public INetChecker Create(CheckerMode type) {
            switch(type) {
                case CheckerMode.КомпонентыСвязности:
                    return new ConnectChecker();
                case CheckerMode.Объем:
                    return new VolumeChecker();
                case CheckerMode.МКЭ:
                    return new FEMChecker();
                default:
                    return new ConnectChecker();
            }
        }
    }
}
