using System;
using System.Collections.Generic;
using System.Text;

namespace NetCheckApp
{
    public enum CheckerMode
    {
        DEFAULT = 1,
        CONNECT_COMPONENT = 2,
        VOLUME = 3,
        FEM = 4
    };

    interface ICheckerFabric
    {
        INetChecker Create(CheckerMode type);
    }

    public class CheckerFabric : ICheckerFabric
    {
        public INetChecker Create(CheckerMode type) {
            switch(type) {
                case CheckerMode.DEFAULT:
                    return new ConnectChecker();
                case CheckerMode.CONNECT_COMPONENT:
                    return new ConnectChecker();
                case CheckerMode.VOLUME:
                    return new VolumeChecker();
                case CheckerMode.FEM:
                    return new FEMChecker();
                default:
                    return new ConnectChecker();
            }
        }
    }
}
