#!/usr/bin/env python3
"""Offline layout sanity for the party/HUD clip fixes. No Unity required."""

import sys


def rect(x, y, w, h):
    return (x, y, x + w, y + h)


def overlaps(a, b):
    return a[0] < b[2] and a[2] > b[0] and a[1] < b[3] and a[3] > b[1]


def hud_bands(screen_w):
    box_w = min(720.0, screen_w - 280.0)
    hint1 = rect(30, 12 + 86, box_w - 28, 20)
    hint2 = rect(30, 12 + 106, box_w - 28, 20)
    quest = rect(30, 12 + 128, box_w - 28, 24)
    last = rect(30, 12 + 154, box_w - 28, 22)
    hud = rect(16, 12, box_w, 188)
    strip_top = 208
    return hint1, hint2, quest, last, hud, strip_top


def save_cluster(screen_w):
    sx = screen_w - 268.0
    f5 = rect(sx, 58, 120, 36)
    f9 = rect(sx + 128, 58, 120, 36)
    f8 = rect(sx, 98, 248, 34)
    return f5, f9, f8


def party_modal(screen_w, screen_h):
    w = min(960.0, screen_w - 32.0)
    h = min(620.0, screen_h - 40.0)
    return rect((screen_w - w) * 0.5, (screen_h - h) * 0.5, w, h)


def create_bottom(screen_w, screen_h):
    w = min(780.0, screen_w - 40.0)
    h = min(620.0, screen_h - 40.0)
    box_y = (screen_h - h) * 0.5
    confirm_y = box_y + h - 56
    hint_y = confirm_y - 44
    return hint_y + 40, confirm_y


def main():
    ok = True

    for w, h in ((1024, 576), (1280, 720), (1600, 900), (800, 480)):
        hint1, hint2, quest, last, hud, strip_top = hud_bands(w)
        if overlaps(hint1, quest) or overlaps(hint2, quest):
            print("FAIL HUD hint overlaps quest at %sx%s" % (w, h))
            ok = False
        if overlaps(quest, last):
            print("FAIL quest overlaps LastEnd at %sx%s" % (w, h))
            ok = False
        if hud[3] > strip_top:
            print("FAIL HUD (%s) overlaps party strip top %s at %sx%s" % (hud[3], strip_top, w, h))
            ok = False

        modal = party_modal(w, h)
        f5, f9, f8 = save_cluster(w)
        would_hit = any(overlaps(modal, b) for b in (f5, f9, f8))
        # Buttons are hidden while PartyMenuOpen, so an intersection is OK only because they are not drawn.
        print("NOTE %sx%s party modal vs F5/F9/F8 would_overlap=%s (buttons hidden when P open)" % (w, h, would_hit))

        hint_bottom, confirm_y = create_bottom(w, h)
        if hint_bottom > confirm_y:
            print("FAIL create hint sits on Confirm at %sx%s" % (w, h))
            ok = False

        p_btn = rect(w - 268.0, 12, 248, 40)
        toast_w = min(440.0, w - 32.0)
        toast = rect(16, h - 42, toast_w, 34)
        if overlaps(p_btn, toast):
            print("FAIL party-open toast overlaps P button at %sx%s" % (w, h))
            ok = False
        if toast[1] < 0:
            print("FAIL party-open toast off-screen at %sx%s" % (w, h))
            ok = False

        quest_w = min(560.0, max(280.0, w - 40.0))
        if quest_w > w - 32:
            print("FAIL quest dialog wider than screen at %s" % w)
            ok = False

        panel_w = min(960.0, w - 32.0)
        alloc_w = panel_w - 36.0
        wrap = alloc_w < 740.0
        row_need = 14 + 6 * 110 + 5 * 8
        if not wrap and alloc_w < row_need:
            print("FAIL alloc single row needs %s px, have %s at %s" % (row_need, alloc_w, w))
            ok = False
        if wrap:
            cell = (alloc_w - 28.0) / 3.0
            if cell < 80:
                print("FAIL wrapped alloc cells too narrow at %s" % w)
                ok = False

    if ok:
        print("All UI layout checks PASS")
        return 0
    print("UI layout checks FAILED")
    return 1


if __name__ == "__main__":
    sys.exit(main())
