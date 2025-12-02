// Copyright 2025 Anar Bastanov
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

namespace Iconify;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        bool ownsMutex = false;
        Mutex? instanceMutex = null;

        try
        {
            instanceMutex = new Mutex(initiallyOwned: false, AppStrings.GlobalMutexName);

            try
            {
                ownsMutex = instanceMutex.WaitOne(TimeSpan.Zero, exitContext: false);
            }
            catch (AbandonedMutexException)
            {
                ownsMutex = true;
            }

            if (!ownsMutex)
                return;

            ApplicationConfiguration.Initialize();
            Application.SetColorMode(SystemColorMode.System);
            Application.Run(new IconifyApp());
        }
        finally
        {
            if (instanceMutex is not null)
            {
                if (ownsMutex)
                {
                    try
                    {
                        instanceMutex.ReleaseMutex();
                    }
                    catch (ApplicationException)
                    {
                    }
                }

                instanceMutex.Dispose();
            }
        }
    }
}
