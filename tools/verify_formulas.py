#!/usr/bin/env python3
"""Mirrors ElementFormulaSelfTest numbers without Unity. CI / agent sanity check."""


def advantage(a: str, d: str) -> bool:
    return {
        ("Earth", "Water"),
        ("Water", "Fire"),
        ("Fire", "Wind"),
        ("Wind", "Earth"),
    }.__contains__((a, d))


def opposite(a: str, b: str) -> bool:
    pair = {a, b}
    return pair == {"Earth", "Fire"} or pair == {"Water", "Wind"}


def skill_e(skill: str, target: str) -> float:
    if skill == "None" or target == "None":
        return 1.0
    if advantage(skill, target):
        return 1.25
    if advantage(target, skill):
        return 0.80
    return 1.0


def normal_e(unit: str, target: str) -> float:
    if unit == "None" or target == "None":
        return 1.0
    if advantage(unit, target):
        return 1.12
    if advantage(target, unit):
        return 0.90
    return 1.0


def mastery(uses: int) -> float:
    if uses <= 0:
        return 0.0
    return min(0.15, (uses // 20) * 0.01)


def clamp_resist(r: float) -> float:
    return min(0.20, max(0.0, r))


def adjacent(el: str) -> set[str]:
    return {
        "Earth": {"Water", "Wind"},
        "Water": {"Earth", "Fire"},
        "Fire": {"Water", "Wind"},
        "Wind": {"Fire", "Earth"},
    }.get(el, set())


def can_learn(unit: str, skill: str) -> bool:
    if unit == "None" or skill == "None":
        return True
    if unit == skill:
        return True
    if opposite(unit, skill):
        return False
    return skill in adjacent(unit)


def main() -> None:
    failed = 0

    def check(name: str, actual: float, expected: float) -> None:
        nonlocal failed
        if abs(actual - expected) > 1e-4:
            failed += 1
            print(f"FAIL {name}: expected {expected}, got {actual}")
        else:
            print(f"PASS {name}: {actual}")

    def expect(name: str, actual: bool, wanted: bool) -> None:
        nonlocal failed
        if actual is not wanted:
            failed += 1
            print(f"FAIL {name}: expected {wanted}")
        else:
            print(f"PASS {name}")

    check("Earth vs Water", skill_e("Earth", "Water"), 1.25)
    check("Water vs Earth", skill_e("Water", "Earth"), 0.80)
    check("Earth vs Fire", skill_e("Earth", "Fire"), 1.00)
    check("Water vs Wind", skill_e("Water", "Wind"), 1.00)
    check("NA Earth vs Water", normal_e("Earth", "Water"), 1.12)
    check("NA Water vs Earth", normal_e("Water", "Earth"), 0.90)
    check("Mastery 20", mastery(20), 0.01)
    check("Mastery 300", mastery(300), 0.15)
    check("Resist 0.25", clamp_resist(0.25), 0.20)
    expect("Earth cannot learn Fire", can_learn("Earth", "Fire"), False)
    expect("Earth can learn Water", can_learn("Earth", "Water"), True)
    check("Opposite learn cost sentinel", -1.0 if not can_learn("Earth", "Fire") else 1.5, -1.0)
    check("Water vs Wind learn cost sentinel", -1.0 if not can_learn("Water", "Wind") else 1.5, -1.0)

    def clamp_mastery(m: float) -> float:
        return min(0.15, max(0.0, m))

    check("M 0.50 clamped", clamp_mastery(0.50), 0.15)
    check("M -0.1 clamped", clamp_mastery(-0.1), 0.0)

    base = 100 * 1.0 - 40 * 0.5
    check("Base", base, 80)
    check("Final skill adv", base * 1.25 * 1 * 1 * 1 * 1, 100)
    check("Final NA", base * 1.12, 89.6)
    check("Final crit", base * 1.25 * 1.5, 150)
    check("Heal E", 1.0, 1.0)
    check("Final clamped M 0.50", base * 1.25 * (1 + clamp_mastery(0.50)), 115)

    magic_base = 100 * 1.0 - 40 * 0.25
    check("Magic Base INT100 DEF40", magic_base, 90)
    check("Magic Final Water vs Fire", magic_base * 1.25, 112.5)

    if failed:
        raise SystemExit(f"{failed} failed")
    print("All formula mirrors PASS")


if __name__ == "__main__":
    main()
