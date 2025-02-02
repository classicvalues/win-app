﻿/*
 * Copyright (c) 2021 Proton Technologies AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Crypto;
using ProtonVPN.Vpn.Common;
using ProtonVPN.Vpn.WireGuard;

namespace ProtonVPN.Vpn.Test.WireGuard
{
    [TestClass]
    public class WireGuardConnectionTest
    {
        [TestMethod]
        public void ConnectShouldFireErrorEventWhenServerPublicKeyIsNullOrEmpty()
        {
            // Assert
            WireGuardConnection wireGuardConnection = GetWireGuardConnection();
            int timesFired = 0;

            wireGuardConnection.StateChanged += (_, e) =>
            {
                if (e.Data.Status == VpnStatus.Disconnected && e.Data.Error == VpnError.MissingServerPublicKey)
                {
                    timesFired++;
                }
            };

            // Act
            wireGuardConnection.Connect(
                new WireGuardEndpoint(new VpnHost("host", "127.0.0.1", "", null)),
                new VpnCredentials("username", "password", "cert",
                    new AsymmetricKeyPair(
                        new SecretKey("U2VjcmV0S2V5", KeyAlgorithm.Unknown),
                        new PublicKey("UHVibGljS2V5", KeyAlgorithm.Unknown))),
                new VpnConfig(new VpnConfigParameters()));

            // Assert
            timesFired.Should().Be(1);
        }

        private WireGuardConnection GetWireGuardConnection()
        {
            ProtonVPN.Common.Configuration.Config config = new();
            ILogger logger = Substitute.For<ILogger>();
            IX25519KeyGenerator xIx25519KeyGenerator = Substitute.For<IX25519KeyGenerator>();
            WireGuardService wireGuardService =
                new(logger, new ProtonVPN.Common.Configuration.Config(), Substitute.For<IService>());
            TrafficManager trafficManager = new("pipe");
            StatusManager statusManager = new(logger, string.Empty);

            return new(logger, config, wireGuardService, trafficManager, statusManager, xIx25519KeyGenerator);
        }
    }
}