from PIL import Image, ImageDraw, ImageFont


def create_grid_image(width, height, spacing):
    # Dynamic output path.
    output_path = f"grid-{width}x{height}-spacing{spacing}.png"

    # Create a white RGB image.
    image = Image.new("RGB", (width, height), "white")
    draw = ImageDraw.Draw(image)
    font = ImageFont.truetype("arial.ttf", 28)

    units_per_grid = 10

    # Draw vertical grid lines and X graduations.
    x_value = 0
    for x in range(0, width, spacing):
        draw.line((x, 0, x, height - 1), fill="black", width=1)

        # Write graduation above the bottom horizontal line,
        # and to the right of the vertical line.
        label = str(x_value)
        bbox = draw.textbbox((0, 0), label, font=font)
        text_x = x + 2
        y = height - 1
        draw.text(
            (text_x, y - 2),
            label,
            fill="black",
            font=font,
            anchor="lb")
        
        x_value += units_per_grid

    # Draw horizontal grid lines aligned from the bottom,
    # and Y graduations.
    y_value = 0
    for offset in range(0, height, spacing):
        y = (height - 1) - offset
        draw.line((0, y, width - 1, y), fill="black", width=1)

        # Write graduation above the horizontal line,
        # and to the right of the left vertical line.
        label = str(y_value)
        text_x = 2
        draw.text(
            (text_x, y - 2),
            label,
            fill="black",
            font=font,
            anchor="lb")

        y_value += units_per_grid

    image.save(output_path)
    return output_path


if __name__ == "__main__":
    width = 3000
    height = 2000
    spacing = 100

    output_file = create_grid_image(width, height, spacing)
    print(f"Saved: {output_file}")