// Copyright Epic Games, Inc. All Rights Reserved.
// This file is automatically generated. Changes to this file may be overwritten.

namespace Epic.OnlineServices.CustomInvites
{
	/// <summary>
	/// Output parameters for the <see cref="OnSendCustomNativeInviteRequestedCallback" /> Function.
	/// </summary>
	public struct SendCustomNativeInviteRequestedCallbackInfo : ICallbackInfo
	{
		/// <summary>
		/// Context that was passed into <see cref="CustomInvitesInterface.AddNotifySendCustomNativeInviteRequested" />
		/// </summary>
		public object ClientData { get; set; }

		/// <summary>
		/// Identifies this event which will need to be acknowledged with <see cref="UI.UIInterface.AcknowledgeEventId" />().
		/// <seealso cref="UI.UIInterface.AcknowledgeEventId" />
		/// </summary>
		public ulong UiEventId { get; set; }

		/// <summary>
		/// The Product User ID of the local user who is inviting.
		/// </summary>
		public ProductUserId LocalUserId { get; set; }

		/// <summary>
		/// The Native Platform Account Type. If only a single integrated platform is configured then
		/// this will always reference that platform.
		/// </summary>
		public Utf8String TargetNativeAccountType { get; set; }

		/// <summary>
		/// The Native Platform Account ID of the target user being invited.
		/// </summary>
		public Utf8String TargetUserNativeAccountId { get; set; }

		/// <summary>
		/// Invite ID that the user is being invited to
		/// </summary>
		public Utf8String InviteId { get; set; }

		public Result? GetResultCode()
		{
			return null;
		}

		internal void Set(ref SendCustomNativeInviteRequestedCallbackInfoInternal other)
		{
			ClientData = other.ClientData;
			UiEventId = other.UiEventId;
			LocalUserId = other.LocalUserId;
			TargetNativeAccountType = other.TargetNativeAccountType;
			TargetUserNativeAccountId = other.TargetUserNativeAccountId;
			InviteId = other.InviteId;
		}
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
	internal struct SendCustomNativeInviteRequestedCallbackInfoInternal : ICallbackInfoInternal, IGettable<SendCustomNativeInviteRequestedCallbackInfo>, ISettable<SendCustomNativeInviteRequestedCallbackInfo>, System.IDisposable
	{
		private System.IntPtr m_ClientData;
		private ulong m_UiEventId;
		private System.IntPtr m_LocalUserId;
		private System.IntPtr m_TargetNativeAccountType;
		private System.IntPtr m_TargetUserNativeAccountId;
		private System.IntPtr m_InviteId;

		public object ClientData
		{
			get
			{
				object value;
				Helper.Get(m_ClientData, out value);
				return value;
			}

			set
			{
				Helper.Set(value, ref m_ClientData);
			}
		}

		public System.IntPtr ClientDataAddress
		{
			get
			{
				return m_ClientData;
			}
		}

		public ulong UiEventId
		{
			get
			{
				return m_UiEventId;
			}

			set
			{
				m_UiEventId = value;
			}
		}

		public ProductUserId LocalUserId
		{
			get
			{
				ProductUserId value;
				Helper.Get(m_LocalUserId, out value);
				return value;
			}

			set
			{
				Helper.Set(value, ref m_LocalUserId);
			}
		}

		public Utf8String TargetNativeAccountType
		{
			get
			{
				Utf8String value;
				Helper.Get(m_TargetNativeAccountType, out value);
				return value;
			}

			set
			{
				Helper.Set(value, ref m_TargetNativeAccountType);
			}
		}

		public Utf8String TargetUserNativeAccountId
		{
			get
			{
				Utf8String value;
				Helper.Get(m_TargetUserNativeAccountId, out value);
				return value;
			}

			set
			{
				Helper.Set(value, ref m_TargetUserNativeAccountId);
			}
		}

		public Utf8String InviteId
		{
			get
			{
				Utf8String value;
				Helper.Get(m_InviteId, out value);
				return value;
			}

			set
			{
				Helper.Set(value, ref m_InviteId);
			}
		}

		public void Set(ref SendCustomNativeInviteRequestedCallbackInfo other)
		{
			ClientData = other.ClientData;
			UiEventId = other.UiEventId;
			LocalUserId = other.LocalUserId;
			TargetNativeAccountType = other.TargetNativeAccountType;
			TargetUserNativeAccountId = other.TargetUserNativeAccountId;
			InviteId = other.InviteId;
		}

		public void Set(ref SendCustomNativeInviteRequestedCallbackInfo? other)
		{
			if (other.HasValue)
			{
				ClientData = other.Value.ClientData;
				UiEventId = other.Value.UiEventId;
				LocalUserId = other.Value.LocalUserId;
				TargetNativeAccountType = other.Value.TargetNativeAccountType;
				TargetUserNativeAccountId = other.Value.TargetUserNativeAccountId;
				InviteId = other.Value.InviteId;
			}
		}

		public void Dispose()
		{
			Helper.Dispose(ref m_ClientData);
			Helper.Dispose(ref m_LocalUserId);
			Helper.Dispose(ref m_TargetNativeAccountType);
			Helper.Dispose(ref m_TargetUserNativeAccountId);
			Helper.Dispose(ref m_InviteId);
		}

		public void Get(out SendCustomNativeInviteRequestedCallbackInfo output)
		{
			output = new SendCustomNativeInviteRequestedCallbackInfo();
			output.Set(ref this);
		}
	}
}