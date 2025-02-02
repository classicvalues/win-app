﻿/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System.Threading;
using System.Threading.Tasks;
using ProtonVPN.UI.Test.Windows;
using ProtonVPN.UI.Test.Results;
using NUnit.Framework;
using ProtonVPN.UI.Test.ApiClient;

namespace ProtonVPN.UI.Test.Tests
{
    [TestFixture]
    public class ConnectionTests : UITestSession
    {
        private readonly LoginWindow _loginWindow = new LoginWindow();
        private readonly MainWindow _mainWindow = new MainWindow();
        private readonly MainWindowResults _mainWindowResults = new MainWindowResults();
        private readonly SettingsWindow _settingsWindow = new SettingsWindow();
        private readonly SettingsResult _settingsResult = new SettingsResult();
        private readonly ConnectionResult _connectionResult = new ConnectionResult();
        private readonly ModalWindow _modalWindow = new ModalWindow();
        private readonly CommonAPI _client = new CommonAPI("http://ip-api.com");

        [Test]
        public async Task ConnectUsingQuickConnectFreeUser()
        {
            TestCaseId = 225;

            _loginWindow.LoginWithFreeUser();
            _mainWindow.QuickConnect();
            _mainWindowResults.CheckIfConnected();
            
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            await _mainWindowResults.CheckIfCorrectCountryIsShownAsync();
        }

        [Test]
        public async Task ConnectUsingQuickConnectBasicUser()
        {
            TestCaseId = 221;

            _loginWindow.LoginWithBasicUser();
            _mainWindow.QuickConnect();
            _mainWindowResults.CheckIfConnected();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            await _mainWindowResults.CheckIfCorrectCountryIsShownAsync();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 222;

            _mainWindow.DisconnectUsingSidebarButton();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public async Task ConnectToFastestViaProfilePlusUser()
        {
            TestCaseId = 225;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickProfilesButton();
            _mainWindow.ConnectToAProfileByName("Fastest");
            _mainWindow.WaitUntilConnected();
            _mainWindowResults.CheckIfConnected();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 229;
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            await _mainWindowResults.CheckIfCorrectCountryIsShownAsync();
        }

        [Test]
        public async Task ConnectToRandomServerViaProfilePlusUser()
        {
            TestCaseId = 225;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickProfilesButton();
            _mainWindow.ConnectToAProfileByName("Random");
            _mainWindow.WaitUntilConnected();
            _mainWindowResults.CheckIfConnected();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
        }

        [Test]
        public void CancelConnectionWhileConnectingPlusUser()
        {
            TestCaseId = 227;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickQuickConnectButton();
            //Pause imitates user delay
            Thread.Sleep(1000);
            _mainWindow.CancelConnection();
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public async Task SelectConnectionByCountryVisionaryUser()
        {
            TestCaseId = 223;

            _loginWindow.LoginWithVisionaryUser();
            _mainWindow.ConnectByCountryName("Ukraine");
            _mainWindowResults.CheckIfConnected();
            await _mainWindowResults.CheckIfCorrectCountryIsShownAsync();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 224;

            _mainWindow.DisconnectByCountryName("Ukraine");
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public void CheckCustomDnsManipulation()
        {
            TestCaseId = 4578;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _settingsWindow.ClickConnectionTab();
            _settingsWindow.EnableCustomDnsServers();
            _settingsWindow.DisableNetshieldForCustomDns();
            _settingsWindow.CloseSettings();
            _mainWindowResults.CheckIfNetshieldIsDisabled();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 4579;

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _settingsWindow.EnterCustomIpv4Address("8.8.8.8");
            _settingsWindow.CloseSettings();
            _mainWindow.QuickConnect();
            _settingsResult.CheckIfDnsAddressMatches("8.8.8.8");
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 4581;

            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _settingsWindow.RemoveDnsAddress();
            _settingsWindow.PressReconnect();
            _mainWindow.WaitUntilConnected();
            _settingsResult.CheckIfDnsAddressDoesNotMatch("8.8.8.8");
        }

        [Test]
        public void CheckIfInvalidDnsIsNotPermitted()
        {
            TestCaseId = 4580;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _settingsWindow.ClickConnectionTab();
            _settingsWindow.EnableCustomDnsServers();
            _settingsWindow.DisableNetshieldForCustomDns();
            _settingsWindow.EnterCustomIpv4Address("1.A.B.4");
            _settingsResult.CheckIfCustomDnsAddressWasNotAdded();
        }

        [Test]
        public void CheckIfConnectionIsRestoredToSameServerAfterAppKill()
        {
            TestCaseId = 217;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.QuickConnect();
            _mainWindowResults.CheckIfSameServerIsKeptAfterKillingApp();
        }

        [Test]
        public void CheckIfKillSwitchIsNotActiveOnAppExit()
        {
            TestCaseId = 216;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.EnableKillSwitch();
            _mainWindow.QuickConnect();
            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickExit();
            _mainWindow.ConfirmAppExit();
            _connectionResult.CheckIfDnsIsResolved();
        }

        [Test]
        public async Task ConnectAndDisconnectViaMap()
        {
            TestCaseId = 219;

            _loginWindow.LoginWithPlusUser();
            _mainWindow.ConnectToCountryViaPin("US");
            _mainWindow.WaitUntilConnected();
            _mainWindowResults.CheckIfConnected();
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
            await _mainWindowResults.CheckIfCorrectCountryIsShownAsync();
            TestRailClient.MarkTestsByStatus();

            TestCaseId = 220;

            _mainWindow.DisconnectFromCountryViaPin("US");
            _mainWindowResults.CheckIfDisconnected();
        }

        [Test]
        public async Task CheckIfIpIsExcludedByIp()
        {
            TestCaseId = 7591;

            _loginWindow.LoginWithPlusUser();
            string homeIpAddress = await _client.GetIpAddress();
            _mainWindow.ClickHamburgerMenu()
                .HamburgerMenu.ClickSettings();
            _modalWindow.MoveModalUp(amountOfTimes: 8);
            _settingsWindow.ClickAdvancedTab();
            _settingsWindow.EnableSplitTunneling();
            _settingsWindow.AddIpAddressSplitTunneling("208.95.112.1");
            _settingsWindow.CloseSettings();
            _mainWindow.QuickConnect();
            await _mainWindowResults.CheckIfIpAddressIsExcluded(homeIpAddress);
        }

        [Test]
        public async Task CheckIfIpIsIncluded()
        {
            TestCaseId = 54967;
            _loginWindow.LoginWithPlusUser();
            _mainWindow.ClickHamburgerMenu()
               .HamburgerMenu.ClickSettings();
            _modalWindow.MoveModalUp(amountOfTimes: 8);
            _settingsWindow.ClickAdvancedTab();
            _settingsWindow.EnableSplitTunneling();
            _settingsWindow.EnableIncludeMode();
            _settingsWindow.AddIpAddressSplitTunneling("208.95.112.1");
            _settingsWindow.CloseSettings();
            _mainWindow.QuickConnect();
            //Delay to get correct IP address
            Thread.Sleep(3000);
            await _mainWindowResults.CheckIfCorrectIPAddressIsShownAsync();
        }

        [SetUp]
        public void TestInitialize()
        {
            CreateSession();
        }

        [TearDown]
        public void TestCleanup()
        {
            TearDown();
        }
    }
}
