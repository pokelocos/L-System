# Creación de árboles 3D con L-System (WIP)
Este proyecto en desarrollo permite, de manera general, crear árboles a través del uso de L-System.

## Características
Este proyecto ofrece a sus usuarios la posibilidad de crear su propia gramática para generar resultados específicos según sus necesidades:
- Permite reglas paramétricas.
- Permite la selección de reglas por condiciones paramétricas.
- Permite la selección de reglas por modo aleatorio/probabilístico (estocástico), basado en sistemas de pesos.
- Permite generar árboles 3D sencillos.
- Permite cortar ramas de los árboles generados y sus ramificaciones derivadas; esto también se refleja en el texto base usado para la generación.

## Crear Gramática
![image](https://github.com/user-attachments/assets/8f131f9f-b6b9-4b7b-b824-a17c896202a2)  
Este proyecto almacena las gramáticas en _SerializableObject_ llamados "Grammar Tree".  
Para crear este tipo de objetos, debe dar clic derecho en las carpetas del proyecto y seleccionar **Crear > Crear Grammar Tree**.  
Esto le permitirá guardar las reglas que definen la gramática con sus condiciones de selección, ya sea con condición paramétrica, probabilísticas (estocásticas) o de contexto.  
La gramática almacena reglas, variables generales interpretables por todas las reglas de la misma gramática y permite marcar si la selección de reglas será estocástica o no.

## Sistema de Derivación y Generación
Este proyecto consta de un _MonoBehaviour_ para la derivación de la gramática y la generación del árbol 3D.  
![image](https://github.com/user-attachments/assets/d58637cd-fb4e-41c5-85f2-a6572556855d)  
En este objeto puede añadir una gramática creada previamente.  
También puede incluir un texto inicial para comenzar la derivación, que se ejecutará y mostrará su resultado en el espacio de salida (output), el cual puede copiar o modificar según sus necesidades.  
Este mismo sistema puede tomar el texto presente en el campo de salida y utilizarlo para la generación de un árbol.

## Árbol y sus Partes
![image](https://github.com/user-attachments/assets/10b0f732-9a14-4618-9418-19132fab1365) Árbol 3D  
![image](https://github.com/user-attachments/assets/afba9c59-b867-409d-a2d9-57e5a217218e) _MonoBehaviour_ Árboles  
![image](https://github.com/user-attachments/assets/2f09f70d-ef56-4cdd-9827-54302b61ad9e) Mono Sub  

### Atención:
Si desea que este proyecto genere elementos específicos, deberá implementar su propia solución de generación.  
Esto puede lograrse modificando manualmente el código principal, **TreeLSystem**, encargado de la ejecución.
