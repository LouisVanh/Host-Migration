// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.Achievements
{
	/// <summary>
	/// Function prototype definition for notifications that come from <see cref="AchievementsInterface.AddNotifyAchievementsUnlocked" />
	/// </summary>
	/// <param name="data">A <see cref="OnAchievementsUnlockedCallbackInfo" /> containing the output information and result</param>
	public delegate void OnAchievementsUnlockedCallback(ref OnAchievementsUnlockedCallbackInfo data);

	[System.Runtime.InteropServices.UnmanagedFunctionPointer(Config.LibraryCallingConvention)]
	internal delegate void OnAchievementsUnlockedCallbackInternal(ref OnAchievementsUnlockedCallbackInfoInternal data);
}