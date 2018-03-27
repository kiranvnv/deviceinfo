using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reactive.Linq;
using SystemConfiguration;
using Foundation;
#if __IOS__
using UIKit;
using CoreTelephony;
using NetworkExtension;
#endif


namespace Plugin.DeviceInfo
{
    public class NetworkImpl : INetwork
    {
#if __IOS__
        //        public IObservable<IWifiScanResult> ScanForWifiNetworks() => Observable.Create<IWifiScanResult>(ob =>
        //        {
        //            //UIDevice.CurrentDevice.CheckSystemVersion(11, 0)
        //            NEHotspotHelper.Register(new NEHotspotHelperOptions(), DispatchQueue.CurrentQueue, handler =>
        //            {
        //                ob.OnNext(new WifiScanResult
        //                {
        //                    // Bssid
        //                    Ssid = handler.Network.Ssid,
        //                    IsSecure = handler.Network.Secure,
        //                    SignalStrength = handler.Network.SignalStrength
        //                });
        //            });
        //            return () => {};
        //        });


        //        public IObservable<Unit> ConnectToWifi(string ssid, string password) => Observable.FromAsync(_ =>
        //        {
        //            var config = new NEHotspotConfiguration(ssid, password, true) { JoinOnce = true };
        //            return NEHotspotConfigurationManager.SharedManager.ApplyConfigurationAsync(config);
        //        });


        public string CellularNetworkCarrier
        {
            get
            {
                using (var info = new CTTelephonyNetworkInfo())
                    return info.SubscriberCellularProvider?.CarrierName;
            }
        }
#else
        //        public IObservable<IWifiScanResult> ScanForWifiNetworks() =>  Observable.Empty<IWifiScanResult>();
        //        public IObservable<Unit> ConnectToWifi(string ssid, string password) => Observable.Empty<Unit>();
        public string CellularNetworkCarrier { get; } = null;
#endif

        public NetworkReachability InternetReachability
        {
            get
            {
                var internet = Reachability.InternetConnectionStatus();

                switch (internet)
                {
                    case NetworkStatus.NotReachable:
                        return NetworkReachability.NotReachable;

                    case NetworkStatus.ReachableViaCarrierDataNetwork:
                        return NetworkReachability.Cellular;

                    case NetworkStatus.ReachableViaWiFiNetwork:
                        return NetworkReachability.Wifi;

                    default:
                        return NetworkReachability.Other;
                }
            }
        }


        public IObservable<NetworkReachability> WhenStatusChanged() => Observable.Create<NetworkReachability>(ob =>
        {
            var handler = new EventHandler((sender, args) =>
                ob.OnNext(this.InternetReachability)
            );
            Reachability.ReachabilityChanged += handler;
            return () => Reachability.ReachabilityChanged -= handler;
        });


#if __IOS__ || __TVOS__
        public string IpAddress => NetworkInterface
            .GetAllNetworkInterfaces()
            .FirstOrDefault(x => x.Name.Equals("en0", StringComparison.InvariantCultureIgnoreCase))?
            .GetIPProperties()
            .UnicastAddresses
            .FirstOrDefault(x => x.Address.AddressFamily == AddressFamily.InterNetwork)?
            .Address?
            .ToString();

        public string WifiSsid
        {
            get
            {
                NSDictionary values;
                var status = CaptiveNetwork.TryCopyCurrentNetworkInfo("en0", out values);
                if (status == StatusCode.NoKey || values == null || !values.ContainsKey(CaptiveNetwork.NetworkInfoKeySSID))
                    return null;

                var ssid = values[CaptiveNetwork.NetworkInfoKeySSID];
                return ssid?.ToString();
            }
        }
#else
        public string IpAddress => null;
        public string WifiSsid => null;
#endif
    }
}