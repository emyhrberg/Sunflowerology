import shutil

# Введіть шлях до файлу, який копіюєте
original_file = "SunflowerWithSeedsTile_Glow.png"

# Ваш список суфіксів
suffixes = ["Sunflower", "Dryflower", "Fireflower",
            "Snowflower",
            "Iceflower",
            "Beachflower",
            "Oceanflower",
            "Jungleflower",
            "Deadflower",
            "Obsidianflower", ]  # Впишіть свій список тут

# Створення копій
for suffix in suffixes:
    new_file = f"{original_file.rsplit('.', 1)[0]}_{suffix}.{original_file.rsplit('.', 1)[1]}"
    shutil.copy(original_file, new_file)
    print(f"Створено: {new_file}")
