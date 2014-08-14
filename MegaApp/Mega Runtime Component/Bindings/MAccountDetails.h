#pragma once

#include "megaapi.h"

namespace mega
{
	public enum class MAccountType
	{
		FREE = 0,
		PROI = 1,
		PROII = 2,
		PROIII = 3
	};

	public ref class MAccountDetails sealed
	{
	public:
		virtual ~MAccountDetails();
		uint64 getUsedStorage();
		uint64 getMaxStorage();
		uint64 getOwnUsedTransfer();
		uint64 getSrvUsedTransfer();
		uint64 getMaxTransfer();
		double getSrvRatio();
		MAccountType getProLevel();
		int getSubscriptionType();
		int64 getProExpiration();

	private:
		MAccountDetails(AccountDetails *accountDetails, bool cMemoryOwn);
		AccountDetails *accountDetails;
		bool cMemoryOwn;
		AccountDetails *getCPtr();
	};
}
