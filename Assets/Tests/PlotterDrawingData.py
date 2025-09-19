import pandas as pd
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import numpy as np
import os
import csv
import re

def int_to_color(color_value):
    """
    Convierte un valor entero a un color RGB.
    """
    colors = {
        0: (1.0, 0.1, 0.1),  # Rojo
        1: (1.0, 1.0, 0.1),  # Amarillo
        2: (0.1, 0.1, 1.0),  # Azul
        3: (0.1, 1.0, 0.1),  # Verde
        4: (0.0, 0.0, 0.0),  # Negro
        5: (0.5, 0.5, 0.5)   # Blanco
    }
    return colors.get(color_value, (1.0, 1.0, 1.0))

def parse_points_flexible(points_str):
    """
    Parsea la cadena de puntos de manera flexible para el formato actual de Unity.
    Extrae todos los números flotantes y los agrupa en tripletas (X, Y, Z).
    """
    points = []
    try:
        # Extraer todos los números flotantes de la cadena
        numbers = re.findall(r'[-+]?\d*\.\d+|\d+', points_str)
        
        # Agrupar los números en tripletas (X, Y, Z)
        for i in range(0, len(numbers), 3):
            if i + 2 < len(numbers):  # Asegurar que tenemos 3 números
                try:
                    x = float(numbers[i])
                    y = float(numbers[i+1])
                    z = float(numbers[i+2])
                    points.append([x, y, z])
                except ValueError:
                    continue
        
        print(f"Parsed {len(points)} points from string")
        
    except Exception as e:
        print(f"Error parsing points flexibly: {e}")
        print(f"Problematic string: {points_str[:100]}...")
    
    return np.array(points) if points else np.array([])

def read_csv_flexible(file_path):
    """
    Lee el archivo CSV de manera flexible.
    """
    data = []
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            # Leer todo el contenido
            content = f.read().splitlines()
            
            # Verificar encabezados
            if not content or len(content) < 2:
                print("CSV file is empty or has no data rows")
                return pd.DataFrame()
            
            headers = content[0].split(',')
            print(f"Headers found: {headers}")
            
            # Procesar cada fila de datos
            for row_num, line in enumerate(content[1:], 1):
                line = line.strip()
                if not line:
                    continue
                
                # Dividir la línea en partes
                parts = line.split(',', 3)  # Dividir en máximo 4 partes
                
                if len(parts) < 4:
                    print(f"Warning: Row {row_num} has only {len(parts)} parts: {parts}")
                    continue
                
                anchor_id = parts[0]
                line_color = parts[1]
                line_size = parts[2]
                line_points = parts[3]  # El resto son los puntos
                
                data.append({
                    'AnchorID': anchor_id,
                    'LineColor': line_color,
                    'LineSize': line_size,
                    'LinePoints': line_points
                })
                
        return pd.DataFrame(data)
    
    except Exception as e:
        print(f"Error reading CSV: {e}")
        return pd.DataFrame()

def plot_drawing_data_fixed(file_path):
    """
    Versión corregida para manejar el formato actual de los datos.
    """
    print(f"Loading CSV file from: {file_path}")
    
    # Leer el archivo
    df = read_csv_flexible(file_path)
    
    if df.empty:
        print("Failed to load data from CSV")
        return
    
    print(f"Successfully loaded {len(df)} rows")
    
    # Preparar el gráfico 3D
    fig = plt.figure(figsize=(14, 10))
    ax = fig.add_subplot(111, projection='3d')
    
    colors_used = set()
    valid_rows = 0
    
    # Variables para calcular los límites de los ejes
    all_x, all_y, all_z = [], [], []
    
    # Iterar a través de cada fila
    for index, row in df.iterrows():
        try:
            # Parsear puntos con el nuevo método flexible
            points_str = str(row['LinePoints'])
            points = parse_points_flexible(points_str)
            
            if len(points) < 2:
                continue
            
            # Recolectar datos para límites de ejes
            all_x.extend(points[:, 0])
            all_y.extend(points[:, 1])
            all_z.extend(points[:, 2])
            
            # Obtener propiedades de la línea
            line_color_int = int(row['LineColor'])
            line_color_rgb = int_to_color(line_color_int)
            colors_used.add(line_color_int)
            
            line_size = float(row['LineSize'])
            
            # Trazar la línea - AJUSTAR EJES PARA UNITY
            # En Unity: X=Right, Y=Up, Z=Forward
            # En matplotlib: podemos asignar como queramos
            ax.plot(points[:, 0], points[:, 2], points[:, 1],  # X, Z, Y para vista similar a Unity
                    color=line_color_rgb, 
                    linewidth=line_size * 2 + 1,
                    alpha=0.8)
            
            valid_rows += 1

        except Exception as e:
            print(f"Error processing row {index}: {e}")
            continue
    
    if valid_rows == 0:
        print("No valid data to plot")
        return
    
    # CONFIGURACIÓN DE EJES Y VISTA
    # Establecer límites de ejes
    if all_x and all_y and all_z:
        margin = 0.1  # Margen del 10%
        x_range = max(all_x) - min(all_x)
        y_range = max(all_y) - min(all_y)
        z_range = max(all_z) - min(all_z)
        
        ax.set_xlim(min(all_x) - margin * x_range, max(all_x) + margin * x_range)
        ax.set_ylim(min(all_z) - margin * z_range, max(all_z) + margin * z_range)  # Nota: y -> z
        ax.set_zlim(min(all_y) - margin * y_range, max(all_y) + margin * y_range)  # Nota: z -> y
    
    # Configurar etiquetas de ejes para coincidir con Unity
    ax.set_xlabel('X (Right/Left)')
    ax.set_ylabel('Z (Forward/Back)')
    ax.set_zlabel('Y (Up/Down)')
    
    ax.set_title('Visualización 3D - Sistema de Coordenadas Unity')
    
    # CONFIGURAR VISTA DE CÁMARA
    # Probar diferentes ángulos de vista
    print("\nOpciones de vista disponibles:")
    print("1. Vista perspectiva (por defecto)")
    print("2. Vista frontal (X-Z)")
    print("3. Vista superior (X-Y)")
    print("4. Vista lateral (Z-Y)")
    
    # Vista por defecto - similar a perspectiva en Unity
    ax.view_init(elev=30, azim=45)  # 30 grados elevación, 45 grados azimuth
    
    # Configurar proporciones de ejes
    ax.set_box_aspect([1, 1, 1])  # Aspecto 1:1:1 para todos los ejes
    
    # Cuadrícula y estilo
    ax.grid(True, alpha=0.3)
    ax.xaxis.pane.fill = False
    ax.yaxis.pane.fill = False
    ax.zaxis.pane.fill = False
    ax.xaxis.pane.set_edgecolor('w')
    ax.yaxis.pane.set_edgecolor('w')
    ax.zaxis.pane.set_edgecolor('w')
    
    # Leyenda de colores
    if colors_used:
        handles = [plt.Line2D([0], [0], color=int_to_color(c), linewidth=4, 
                             label=f'Color {c}') 
                  for c in sorted(colors_used)]
        ax.legend(handles=handles, loc='upper left', bbox_to_anchor=(1.05, 1))
    
    plt.tight_layout()
    plt.show()
    
    # Función para vistas predefinidas
    def create_alternative_views():
        """Crear vistas alternativas"""
        views = [
            ('Vista Perspectiva', 30, 45),
            ('Vista Frontal (X-Z)', 0, 0),
            ('Vista Superior (X-Y)', 90, 0),
            ('Vista Lateral (Z-Y)', 0, 90),
            ('Vista Isométrica', 35, 45)
        ]
        
        for name, elev, azim in views:
            fig = plt.figure(figsize=(10, 8))
            ax = fig.add_subplot(111, projection='3d')
            
            # Replotear todos los datos
            for index, row in df.iterrows():
                points_str = str(row['LinePoints'])
                points = parse_points_flexible(points_str)
                if len(points) >= 2:
                    line_color_int = int(row['LineColor'])
                    line_color_rgb = int_to_color(line_color_int)
                    line_size = float(row['LineSize'])
                    
                    ax.plot(points[:, 0], points[:, 2], points[:, 1],
                            color=line_color_rgb, 
                            linewidth=line_size * 2 + 1,
                            alpha=0.8)
            
            ax.view_init(elev=elev, azim=azim)
            ax.set_xlabel('X (Right/Left)')
            ax.set_ylabel('Z (Forward/Back)')
            ax.set_zlabel('Y (Up/Down)')
            ax.set_title(f'{name}\nElevación: {elev}°, Azimuth: {azim}°')
            ax.grid(True, alpha=0.3)
            plt.tight_layout()
            plt.show()
    
    # Preguntar si quiere ver vistas alternativas
    response = input("\n¿Quieres ver vistas alternativas? (s/n): ")
    if response.lower() == 's':
        create_alternative_views()
    
    print(f"\nEstadísticas:")
    print(f"Total de trazos procesados: {valid_rows}/{len(df)}")
    print(f"Colores utilizados: {sorted(colors_used)}")

# Añade esta función para interactuar con la vista
def interactive_view_controls():
    """Explicación de controles de vista interactiva"""
    print("\n" + "="*50)
    print("CONTROLES INTERACTIVOS DE VISTA:")
    print("="*50)
    print("Click y arrastrar: Rotar la vista")
    print("Ctrl + Click y arrastrar: Zoom")
    print("Shift + Click y arrastrar: Pan")
    print("Tecla 'r': Resetear vista")
    print("Tecla 'e': Cambiar elevación")
    print("Tecla 'a': Cambiar azimuth")
    print("="*50)

def plot_drawing_data_points_only(file_path):
    """
    Versión que muestra solo puntos en lugar de líneas continuas.
    """
    print(f"Loading CSV file from: {file_path}")
    
    # Leer el archivo
    df = read_csv_flexible(file_path)
    
    if df.empty:
        print("Failed to load data from CSV")
        return
    
    print(f"Successfully loaded {len(df)} rows")
    
    # Preparar el gráfico 3D
    fig = plt.figure(figsize=(14, 10))
    ax = fig.add_subplot(111, projection='3d')
    
    colors_used = set()
    valid_rows = 0
    total_points = 0
    
    # Variables para calcular los límites de los ejes
    all_x, all_y, all_z = [], [], []
    
    # Iterar a través de cada fila
    for index, row in df.iterrows():
        try:
            # Parsear puntos
            points_str = str(row['LinePoints'])
            points = parse_points_flexible(points_str)
            
            if len(points) < 1:  # Cambiado a 1 punto mínimo
                continue
            
            # Recolectar datos para límites de ejes
            all_x.extend(points[:, 0])
            all_y.extend(points[:, 1])
            all_z.extend(points[:, 2])
            
            # Obtener propiedades
            line_color_int = int(row['LineColor'])
            line_color_rgb = int_to_color(line_color_int)
            colors_used.add(line_color_int)
            
            line_size = float(row['LineSize'])
            
            # MOSTRAR SOLO PUNTOS - NO LÍNEAS
            # Scatter plot para puntos individuales
            ax.scatter(points[:, 0], points[:, 2], points[:, 1],  # X, Z, Y para Unity
                      color=line_color_rgb, 
                      s=line_size * 25 + 0,  # Tamaño de los puntos
                      alpha=0.7,
                      edgecolors='w',  # Borde blanco para mejor visibilidad
                      linewidth=0.1,
                      label=f"Color {line_color_int}" if line_color_int not in colors_used else "")
            
            valid_rows += 1
            total_points += len(points)
            
            print(f"Plotted {len(points)} points for AnchorID: {row['AnchorID']}")

        except Exception as e:
            print(f"Error processing row {index}: {e}")
            continue
    
    if valid_rows == 0:
        print("No valid data to plot")
        return
    
    # CONFIGURACIÓN DE EJES Y VISTA
    if all_x and all_y and all_z:
        margin = 0.1
        x_range = max(all_x) - min(all_x)
        y_range = max(all_y) - min(all_y)
        z_range = max(all_z) - min(all_z)
        
        ax.set_xlim(min(all_x) - margin * x_range, max(all_x) + margin * x_range)
        ax.set_ylim(min(all_z) - margin * z_range, max(all_z) + margin * z_range)
        ax.set_zlim(min(all_y) - margin * y_range, max(all_y) + margin * y_range)
    
    # Configurar etiquetas de ejes
    ax.set_xlabel('X (Right/Left)')
    ax.set_ylabel('Z (Forward/Back)')
    ax.set_zlabel('Y (Up/Down)')
    
    ax.set_title(f'Visualización de Puntos 3D\nTotal: {total_points} puntos, {valid_rows} trazos')
    
    # Configurar vista
    ax.view_init(elev=30, azim=45)
    ax.set_box_aspect([1, 1, 1])
    
    # Cuadrícula y estilo
    ax.grid(True, alpha=0.3)
    ax.xaxis.pane.fill = False
    ax.yaxis.pane.fill = False
    ax.zaxis.pane.fill = False
    
    # Leyenda de colores (sin duplicados)
    if colors_used:
        unique_labels = {}
        for color_int in sorted(colors_used):
            color_name = ["Rojo", "Amarillo", "Azul", "Verde", "Negro", "Blanco"][color_int]
            unique_labels[color_int] = f'Color {color_int} ({color_name})'
        
        handles = [plt.Line2D([0], [0], marker='o', color='w', 
                             markerfacecolor=int_to_color(c), markersize=10,
                             label=unique_labels[c]) 
                  for c in sorted(colors_used)]
        ax.legend(handles=handles, loc='upper left', bbox_to_anchor=(1.05, 1))
    
    plt.tight_layout()
    plt.show()
    
    print(f"\nEstadísticas:")
    print(f"Total de trazos: {valid_rows}/{len(df)}")
    print(f"Total de puntos: {total_points}")
    print(f"Colores utilizados: {sorted(colors_used)}")
    print(f"Puntos por trazo: {total_points/valid_rows:.1f} en promedio")

# Versión alternativa: puntos con tamaño variable por trazo
def plot_drawing_data_points_variable_size(file_path):
    """
    Versión con puntos de tamaño variable según el trazo.
    """
    print(f"Loading CSV file from: {file_path}")
    
    df = read_csv_flexible(file_path)
    if df.empty:
        return
    
    fig = plt.figure(figsize=(14, 10))
    ax = fig.add_subplot(111, projection='3d')
    
    colors_used = set()
    total_points = 0
    
    # Para cada trazo, plotear puntos con tamaño basado en LineSize
    for index, row in df.iterrows():
        try:
            points_str = str(row['LinePoints'])
            points = parse_points_flexible(points_str)
            
            if len(points) < 1:
                continue
            
            line_color_int = int(row['LineColor'])
            line_color_rgb = int_to_color(line_color_int)
            colors_used.add(line_color_int)
            
            line_size = float(row['LineSize'])
            
            # Puntos con tamaño variable según LineSize
            ax.scatter(points[:, 0], points[:, 2], points[:, 1],
                      color=line_color_rgb,
                      s=line_size * 150 + 30,  # Tamaño más grande para mejor visibilidad
                      alpha=0.6,
                      edgecolors='black',
                      linewidth=0.3)
            
            total_points += len(points)
            
        except Exception as e:
            continue
    
    # Configuración del gráfico
    ax.set_xlabel('X (Right/Left)')
    ax.set_ylabel('Z (Forward/Back)')
    ax.set_zlabel('Y (Up/Down)')
    ax.set_title(f'Puntos 3D - Tamaño variable por trazo\nTotal: {total_points} puntos')
    
    ax.view_init(elev=30, azim=45)
    ax.grid(True, alpha=0.2)
    
    # Leyenda
    if colors_used:
        handles = [plt.Line2D([0], [0], marker='o', color='w', 
                             markerfacecolor=int_to_color(c), markersize=8,
                             label=f'Color {c}') 
                  for c in sorted(colors_used)]
        ax.legend(handles=handles, loc='upper left', bbox_to_anchor=(1.05, 1))
    
    plt.tight_layout()
    plt.show()

if __name__ == '__main__':
    script_dir = os.path.dirname(os.path.abspath(__file__))
    csv_file = os.path.join(script_dir, 'drawing_data.csv')
    
    if not os.path.exists(csv_file):
        print(f"Error: File not found")
    else:
        print("Selecciona el modo de visualización:")
        print("1. Solo puntos")
        print("2. Puntos con tamaño variable")
        print("3. Líneas continuas (original)")
        
        choice = input("Opción (1-3): ").strip()
        
        if choice == "1":
            plot_drawing_data_points_only(csv_file)
        elif choice == "2":
            plot_drawing_data_points_variable_size(csv_file)
        else:
            plot_drawing_data_fixed(csv_file)  # Función original con líneas