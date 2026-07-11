using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class CInAppPurchaseCocoa : ICInAppPurchase
{
	private bool m_isInitialized;

	public override void Init(string[] products, string marketPublicKey)
	{
		IAPNativeInit(products, new string[products.Length]);
		m_isInitialized = true;
	}

	public override void Init(Dictionary<string, string> products, string marketPublicKey)
	{
		string[] array = new string[products.Keys.Count];
		string[] array2 = new string[products.Values.Count];
		products.Keys.CopyTo(array, 0);
		products.Values.CopyTo(array2, 0);
		IAPNativeInit(array, array2);
		m_isInitialized = true;
	}

	public override CInAppPurchaseProduct[] GetAvailableProducts()
	{
		if (m_isInitialized)
		{
			string src = Marshal.PtrToStringAnsi(IAPNativeRequestProductData());
			int amountOfValuesFromStringForKey = CStringUtils.GetAmountOfValuesFromStringForKey(src, "Product");
			if (amountOfValuesFromStringForKey > 0)
			{
				CInAppPurchaseProduct[] array = null;
				array = new CInAppPurchaseProduct[amountOfValuesFromStringForKey];
				for (int i = 0; i < amountOfValuesFromStringForKey; i++)
				{
					array[i] = new CInAppPurchaseProduct(CStringUtils.ExtractFromStringForKeyValue(src, "Product", i + 1));
				}
				return array;
			}
			return null;
		}
		return null;
	}

	public override void BuyProduct(string product)
	{
		if (m_isInitialized)
		{
			IAPNativeBuyProduct(product);
		}
	}

	public override bool IsTurnedOn()
	{
		return IAPNativeIsTurnedOn();
	}

	public override bool IsAvailable()
	{
		return true;
	}

	public override string RetrieveProduct()
	{
		if (m_isInitialized)
		{
			return Marshal.PtrToStringAnsi(IAPNativeRetrieveProduct());
		}
		return null;
	}

	public override TRANSACTION_STATE GetPurchaseTransactionStatus()
	{
		if (m_isInitialized)
		{
			return (TRANSACTION_STATE)IAPNativeGetTransactionStatus();
		}
		return TRANSACTION_STATE.NONE;
	}

	public override void RestoreCompletedTransactions()
	{
		if (m_isInitialized)
		{
			IAPNativeRestoreCompletedTransactions();
		}
	}

	public override RESTORE_STATE GetRestoreStatus()
	{
		if (m_isInitialized)
		{
			return (RESTORE_STATE)IAPNativeGetRestoreStatus();
		}
		return RESTORE_STATE.NONE;
	}

	public override int GetRetrievalQueueCount()
	{
		if (m_isInitialized)
		{
			return IAPNativeGetRetrievalQueueCount();
		}
		return 0;
	}

	public override string GetRetrievalQueueItem(int index)
	{
		if (m_isInitialized)
		{
			return Marshal.PtrToStringAnsi(IAPNativeGetRetrievalQueueItem(index));
		}
		return null;
	}

	public override void RetrievalQueueDispose(int numItems)
	{
		if (m_isInitialized)
		{
			IAPNativeRetrievalQueueDispose(numItems);
		}
	}

#if UNITY_IOS
	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	private static extern void IAPNativeInit(string[] products, string[] ids);
#else
	private static void IAPNativeInit(string[] products, string[] ids) { }
#endif

#if UNITY_IOS
	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	private static extern IntPtr IAPNativeRequestProductData();
#else
	private static IntPtr IAPNativeRequestProductData() { return IntPtr.Zero; }
#endif

#if UNITY_IOS
	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	private static extern void IAPNativeBuyProduct(string product);
#else
	private static void IAPNativeBuyProduct(string product) { }
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern int IAPNativeGetTransactionStatus();
#else
	private static int IAPNativeGetTransactionStatus() { return 0; }
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern int IAPNativeRestoreCompletedTransactions();
#else
	private static int IAPNativeRestoreCompletedTransactions() { return 0; }
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern int IAPNativeGetRestoreStatus();
#else
	private static int IAPNativeGetRestoreStatus() { return 0; }
#endif

#if UNITY_IOS
	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	private static extern IntPtr IAPNativeRetrieveProduct();
#else
	private static IntPtr IAPNativeRetrieveProduct() { return IntPtr.Zero; }
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern bool IAPNativeIsTurnedOn();
#else
	private static bool IAPNativeIsTurnedOn() { return false; }
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern int IAPNativeGetRetrievalQueueCount();
#else
	private static int IAPNativeGetRetrievalQueueCount() { return 0; }
#endif

#if UNITY_IOS
	[DllImport("__Internal", CharSet = CharSet.Ansi)]
	private static extern IntPtr IAPNativeGetRetrievalQueueItem(int index);
#else
	private static IntPtr IAPNativeGetRetrievalQueueItem(int index) { return IntPtr.Zero; }
#endif

#if UNITY_IOS
	[DllImport("__Internal")]
	private static extern void IAPNativeRetrievalQueueDispose(int numItems);
#else
	private static void IAPNativeRetrievalQueueDispose(int numItems) { }
#endif
}
