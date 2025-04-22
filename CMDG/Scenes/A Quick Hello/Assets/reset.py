import os

def reset_cmd_color_to_dark_grey():
    # 08 = black background (0) and dark grey foreground (8)
    os.system("color 07")

if __name__ == "__main__":
    reset_cmd_color_to_dark_grey()
