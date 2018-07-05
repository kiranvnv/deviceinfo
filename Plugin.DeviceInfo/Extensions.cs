﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;


namespace Plugin.DeviceInfo
{
    public static class Extensions
    {
        public static IObservable<Unit> WhenConnected(this INetwork network) => network
            .WhenNetworkTypeChanged()
            .Where(x => x != NetworkType.NotReachable)
            .Select(_ => Unit.Default);


        public static IObservable<Unit> WhenDisconnected(this INetwork network) => network
            .WhenNetworkTypeChanged()
            .Where(x => x == NetworkType.NotReachable)
            .Select(_ => Unit.Default);
        public static Task<int> ReadPercentage(this IPowerState power) => power
            .WhenBatteryPercentageChanged()
            .Take(1)
            .ToTask();


        public static Task<PowerStatus> ReadPowerStatus(this IPowerState power) => power
            .WhenPowerStatusChanged()
            .Take(1)
            .ToTask();
    }
}
