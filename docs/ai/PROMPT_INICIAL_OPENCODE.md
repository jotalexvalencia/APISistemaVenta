```markdown
# PROMPT INICIAL PARA OPENCODE + QWEN LOCAL

Actúa como asistente técnico local para dockerizar este proyecto.

Usa como contexto estos archivos:

- /docs/ai/AGENTS.md
- /docs/ai/ENGRAM.md
- /docs/ai/SKILLS.md
- /docs/ai/MCP.md

Objetivo:
Dockerizar progresivamente un proyecto .NET 10 + Angular 16 + SQL Server.

Reglas:
- No inventes estructura.
- Primero inspecciona el repo.
- No hagas cambios masivos.
- No borres archivos.
- No ejecutes comandos destructivos.
- No uses `dotnet publish /t:PublishContainer` porque puede fallar con junctions en Windows.
- Usa Dockerfile multi-stage.
- Documenta cada avance con la plantilla de aprendizaje.

Primera tarea:
Haz una auditoría inicial del repositorio.

Necesito que entregues:

1. Estructura detectada del proyecto.
2. Archivos relevantes encontrados.
3. Riesgos para Docker.
4. Plan de dockerización en fases.
5. Primer cambio recomendado.
6. Comandos de verificación.
7. Qué documentación debo crear.

No modifiques archivos todavía.
Primero responde con el diagnóstico.
```