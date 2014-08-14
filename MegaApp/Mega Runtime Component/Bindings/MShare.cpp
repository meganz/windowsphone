#include "MShare.h"

using namespace mega;
using namespace Platform;

MShare::MShare(MegaShare *megaShare, bool cMemoryOwn)
{
	this->megaShare = megaShare;
	this->cMemoryOwn = cMemoryOwn;
}

MShare::~MShare()
{
	if (cMemoryOwn)
		delete megaShare;
}

MegaShare * MShare::getCPtr()
{
	return megaShare;
}

String^ MShare::getUser()
{
	if (!megaShare) return nullptr;

	std::string utf16user;
	const char *utf8user = megaShare->getUser();
	MegaApi::utf8ToUtf16(utf8user, &utf16user);

	return ref new String((wchar_t *)utf16user.data());
}

uint64 MShare::getNodeHandle()
{
	return megaShare ? megaShare->getNodeHandle() : ::mega::UNDEF;
}

int MShare::getAccess()
{
	return megaShare ? megaShare->getAccess() : MegaShare::ACCESS_UNKNOWN;
}

int MShare::getTimestamp()
{
	return megaShare ? megaShare->getTimestamp() : 0;
}

