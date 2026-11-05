# shellcheck shell=bash
# A system "isn't responding" (ANR) or "has stopped" dialog takes the screen's taps and keys until someone answers it.
# hide_error_dialogs stops new ones, not one that is already showing.

# close_error_dialogs <serial>
# Closes each ANR or app-error dialog on the screen (force-stop is safe for a launcher: Android restarts it). Prints an
# `Emulator` warning per dialog, naming its package; classify-shard-failure.sh keys off its text.
close_error_dialogs() {
  local serial="$1" dialogs dialog method closed
  for _ in 1 2 3; do
    dialogs=$(error_dialogs "$serial")
    [[ -n "$dialogs" ]] || return 0
    dialog="${dialogs%%$'\n'*}"
    closed=""
    for method in "tapping Wait" "force-stopping its process" "broadcasting CLOSE_SYSTEM_DIALOGS"; do
      case "$method" in
        "tapping Wait") tap_wait "$serial" ;;
        force-stopping*) timeout 20 adb -s "$serial" shell am force-stop "${dialog##* }" > /dev/null 2>&1 || true ;;
        broadcasting*)
          timeout 20 adb -s "$serial" shell am broadcast -a android.intent.action.CLOSE_SYSTEM_DIALOGS > /dev/null 2>&1 || true ;;
      esac
      sleep 2
      dialogs=$(error_dialogs "$serial")
      if [[ $'\n'"$dialogs"$'\n' != *$'\n'"$dialog"$'\n'* ]]; then
        closed="$method"
        break
      fi
    done
    if [[ -z "$closed" ]]; then
      echo "::warning title=Emulator::$(describe "$dialog") is still on the screen after Wait, force-stop and CLOSE_SYSTEM_DIALOGS."
      return 0
    fi
    echo "::warning title=Emulator::$(describe "$dialog") was on the screen; closed it by $closed."
  done
}

error_dialogs() {
  { timeout 20 adb -s "$1" shell dumpsys window windows 2> /dev/null || true; } | tr -d '\r' |
    sed -n -E -e 's/^ *Window #[0-9]+ Window\{[^ ]+ u[0-9]+ Application Not Responding: ([^:}]+)[^}]*\}:$/ANR \1/p' \
      -e 's/^ *Window #[0-9]+ Window\{[^ ]+ u[0-9]+ Application Error: ([^:}]+)[^}]*\}:$/app-error \1/p'
}

describe() {
  echo "An ${1%% *} dialog for ${1##* }"
}

tap_wait() {
  local xml
  timeout 30 adb -s "$1" shell uiautomator dump /data/local/tmp/error-dialog.xml > /dev/null 2>&1 || return 0
  xml=$(timeout 10 adb -s "$1" shell cat /data/local/tmp/error-dialog.xml 2> /dev/null || true)
  [[ "$xml" =~ resource-id=\"android:id/aerr_wait\"[^\>]*bounds=\"\[([0-9]+),([0-9]+)\]\[([0-9]+),([0-9]+)\]\" ]] || return 0
  timeout 10 adb -s "$1" shell input tap $(((BASH_REMATCH[1] + BASH_REMATCH[3]) / 2)) $(((BASH_REMATCH[2] + BASH_REMATCH[4]) / 2)) \
    > /dev/null 2>&1 || true
}
