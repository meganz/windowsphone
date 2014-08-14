#include "MAccountDetails.h"

using namespace mega;
using namespace Platform;

MAccountDetails::MAccountDetails(AccountDetails *accountDetails, bool cMemoryOwn)
{
	this->accountDetails = accountDetails;
	this->cMemoryOwn;
}

MAccountDetails::~MAccountDetails()
{
	if (cMemoryOwn)
		delete accountDetails;
}

AccountDetails* MAccountDetails::getCPtr()
{
	return accountDetails;
}

uint64 MAccountDetails::getUsedStorage() 
{
	return accountDetails ? accountDetails->storage_used : 0;
}

uint64 MAccountDetails::getMaxStorage() 
{
	return accountDetails ? accountDetails->storage_max : 0;
}

uint64 MAccountDetails::getOwnUsedTransfer() 
{
	return accountDetails ? accountDetails->transfer_own_used : 0;
}

uint64 MAccountDetails::getSrvUsedTransfer()
{
	return accountDetails ? accountDetails->transfer_srv_used : 0;
}

uint64 MAccountDetails::getMaxTransfer()
{
	return accountDetails ? accountDetails->transfer_max : 0;
}

double MAccountDetails::getSrvRatio()
{
	return accountDetails ? accountDetails->srv_ratio : 0;
}

MAccountType MAccountDetails::getProLevel()
{
	return (MAccountType) (accountDetails ? accountDetails->pro_level : 0);
}

int MAccountDetails::getSubscriptionType()
{
	return accountDetails ? accountDetails->subscription_type : 0;
}

int64 MAccountDetails::getProExpiration()
{
	return accountDetails ? accountDetails->pro_until : 0;
}

