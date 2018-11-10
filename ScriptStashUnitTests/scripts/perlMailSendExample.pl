#!/usr/bin/perl
use strict;
use warnings;
# source code from: https://learn.perl.org/examples/email.html

# first, create your message
use Email::MIME;
my $message = Email::MIME->create(
  header_str => [
    From    => 'you@example.com',
    To      => 'friend@example.com',
    Subject => 'Happy birthday!',
  ],
  attributes => {
    encoding => 'quoted-printable',
    charset  => 'ISO-8859-1',
  },
);

# send the message
use Email::Sender::Simple qw(sendmail);
sendmail($message);