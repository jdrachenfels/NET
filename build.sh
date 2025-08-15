#!/bin/sh -eu
buildah bud --layers -f SecureMailHandler.Dockerfile \
	-t harbor.intern.drachenfels.de/shieldos/securemail:latest
