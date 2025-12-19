import sys
import importlib
import pkgutil
import traceback

PACKAGE_NAME = "Face_identify_app"

CORE_DEPS = [
    "mediapipe",
    "numpy",
    "cv2",
]

def check_python():
    print("=" * 60)
    print("Python environment")
    print("=" * 60)
    print("Executable :", sys.executable)
    print("Version    :", sys.version)
    print()

def check_core_dependencies():
    print("=" * 60)
    print("Core dependencies check")
    print("=" * 60)
    for dep in CORE_DEPS:
        try:
            m = importlib.import_module(dep)
            ver = getattr(m, "__version__", "unknown")
            print(f"✔ {dep} imported (version: {ver})")
        except Exception:
            print(f"❌ {dep} FAILED")
            traceback.print_exc()
    print()

def check_package_imports():
    print("=" * 60)
    print(f"Package scan: {PACKAGE_NAME}")
    print("=" * 60)

    try:
        pkg = importlib.import_module(PACKAGE_NAME)
        print(f"✔ Root package imported: {PACKAGE_NAME}")
    except Exception:
        print(f"❌ Cannot import root package: {PACKAGE_NAME}")
        traceback.print_exc()
        return

    print()

    for finder, name, ispkg in pkgutil.walk_packages(
        pkg.__path__, pkg.__name__ + "."
    ):
        try:
            importlib.import_module(name)
            print(f"✔ {name}")
        except Exception:
            print(f"❌ {name}")
            traceback.print_exc()

if __name__ == "__main__":
    check_python()
    check_core_dependencies()
    check_package_imports()
