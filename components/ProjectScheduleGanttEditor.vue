<template>
  <section class="gantt-editor">
    <header class="gantt-editor__header">
      <div>
        <p class="gantt-editor__eyebrow">Cronograma de projetos</p>
        <h1 class="gantt-editor__title">{{ projectName }}</h1>
      </div>
      <div class="gantt-editor__actions">
        <button class="btn btn--ghost" type="button" @click="zoomOut">- Zoom</button>
        <button class="btn btn--ghost" type="button" @click="zoomIn">+ Zoom</button>
        <button class="btn btn--primary" type="button" @click="addTask">Adicionar tarefa</button>
      </div>
    </header>

    <div class="gantt-editor__workspace">
      <aside class="gantt-editor__grid">
        <div class="grid-header">
          <span>ID</span>
          <span>Nome</span>
          <span>Responsável</span>
          <span>Início</span>
          <span>Fim</span>
          <span>%</span>
          <span>Ações</span>
        </div>
        <div
          v-for="task in tasks"
          :key="task.id"
          class="grid-row"
        >
          <span class="grid-cell">{{ task.code }}</span>
          <div class="grid-name" :style="{ paddingLeft: `${task.level * 16}px` }">
            <input v-model="task.name" class="grid-input" />
          </div>
          <input v-model="task.owner" class="grid-input" />
          <input v-model="task.start" type="date" class="grid-input" />
          <input v-model="task.end" type="date" class="grid-input" />
          <input v-model.number="task.progress" type="number" min="0" max="100" class="grid-input" />
          <div class="grid-actions">
            <button
              class="btn btn--ghost"
              type="button"
              @click="outdentTask(task.id)"
              :disabled="task.level === 0"
            >
              Recuar
            </button>
            <button class="btn btn--ghost" type="button" @click="indentTask(task.id)">
              Avançar
            </button>
            <button class="btn btn--ghost" type="button" @click="removeTask(task.id)">
              Remover
            </button>
          </div>
        </div>
      </aside>

      <div class="gantt-editor__timeline-wrapper">
        <main class="gantt-editor__timeline">
          <div class="timeline-header" :style="{ gridTemplateColumns: timelineColumns }">
            <div
              v-for="day in timeline"
              :key="day.iso"
              class="timeline-header__cell"
            >
              <span class="timeline-header__day">{{ day.day }}</span>
              <span class="timeline-header__label">{{ day.label }}</span>
            </div>
          </div>

          <div class="timeline-body" :style="{ gridTemplateColumns: timelineColumns }">
            <div
              v-for="task in tasks"
              :key="task.id"
              class="timeline-row"
            >
              <div
                class="timeline-row__bar"
                :style="barStyle(task)"
              >
                <span>{{ task.name }}</span>
                <small>{{ task.progress }}%</small>
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  </section>
</template>

<script setup>
import { computed, ref } from "vue";

const props = defineProps({
  projectName: {
    type: String,
    default: "Programa de Transformação Digital",
  },
  initialTasks: {
    type: Array,
    default: () => [
      {
        id: 1,
        code: "T-101",
        name: "Levantamento de requisitos",
        owner: "Fernanda",
        start: "2024-09-02",
        end: "2024-09-13",
        progress: 65,
      },
      {
        id: 2,
        code: "T-102",
        name: "Desenho da solução",
        owner: "Rafael",
        start: "2024-09-16",
        end: "2024-09-27",
        progress: 30,
      },
      {
        id: 3,
        code: "T-103",
        name: "Configuração do ambiente",
        owner: "Giovana",
        start: "2024-09-18",
        end: "2024-10-04",
        progress: 10,
      },
    ],
  },
});

const tasks = ref(
  structuredClone(props.initialTasks).map((task) => ({
    ...task,
    level: task.level ?? 0,
    parentId: task.parentId ?? null,
  })),
);
const zoomLevel = ref(1);

const parseDate = (value) => {
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? null : parsed;
};

const timeline = computed(() => {
  const dates = tasks.value
    .flatMap((task) => [parseDate(task.start), parseDate(task.end)])
    .filter(Boolean);
  const fallbackStart = new Date("2024-09-01");
  const fallbackEnd = new Date("2024-10-15");
  const minDate = dates.length ? new Date(Math.min(...dates.map((date) => date.getTime()))) : fallbackStart;
  const maxDate = dates.length ? new Date(Math.max(...dates.map((date) => date.getTime()))) : fallbackEnd;
  minDate.setDate(minDate.getDate() - 3);
  maxDate.setDate(maxDate.getDate() + 3);
  const days = [];
  const current = new Date(minDate);

  while (current <= maxDate) {
    const iso = current.toISOString().slice(0, 10);
    const day = current.getDate();
    const label = current.toLocaleDateString("pt-BR", { month: "short" });
    days.push({ iso, day, label });
    current.setDate(current.getDate() + 1);
  }

  return days;
});

const timelineColumns = computed(() => `repeat(${timeline.value.length}, ${28 * zoomLevel.value}px)`);

const addTask = () => {
  const nextId = Math.max(0, ...tasks.value.map((task) => task.id)) + 1;
  tasks.value.push({
    id: nextId,
    code: `T-${100 + nextId}`,
    name: "Nova tarefa",
    owner: "",
    start: timeline.value[0].iso,
    end: timeline.value[5].iso,
    progress: 0,
    level: 0,
    parentId: null,
  });
};

const removeTask = (id) => {
  tasks.value = tasks.value.filter((task) => task.id !== id);
};

const indentTask = (id) => {
  const index = tasks.value.findIndex((task) => task.id === id);
  if (index <= 0) {
    return;
  }

  const previous = tasks.value[index - 1];
  const current = tasks.value[index];
  const nextLevel = Math.min(previous.level + 1, 6);

  tasks.value[index] = {
    ...current,
    level: nextLevel,
    parentId: previous.id,
  };
};

const outdentTask = (id) => {
  const index = tasks.value.findIndex((task) => task.id === id);
  if (index < 0) {
    return;
  }

  const current = tasks.value[index];
  const nextLevel = Math.max(current.level - 1, 0);
  const parentIndex = tasks.value
    .slice(0, index)
    .reverse()
    .findIndex((task) => task.level === nextLevel - 1);
  const parentTask =
    nextLevel > 0 && parentIndex !== -1
      ? tasks.value[index - parentIndex - 1]
      : null;

  tasks.value[index] = {
    ...current,
    level: nextLevel,
    parentId: parentTask?.id ?? null,
  };
};

const zoomIn = () => {
  zoomLevel.value = Math.min(2.5, Number((zoomLevel.value + 0.2).toFixed(1)));
};

const zoomOut = () => {
  zoomLevel.value = Math.max(0.6, Number((zoomLevel.value - 0.2).toFixed(1)));
};

const barStyle = (task) => {
  const startIndex = timeline.value.findIndex((day) => day.iso === task.start);
  const endIndex = timeline.value.findIndex((day) => day.iso === task.end);
  const safeStart = Math.max(startIndex, 0);
  const safeEnd = Math.max(endIndex, safeStart + 1);
  const gridStart = safeStart + 1;
  const gridEnd = safeEnd + 2;

  return {
    gridColumn: `${gridStart} / ${gridEnd}`,
    background: `linear-gradient(90deg, #2f80ed ${task.progress}%, #dfe9f7 ${task.progress}%)`,
  };
};
</script>

<style scoped>
:root {
  color-scheme: light;
}

.gantt-editor {
  display: flex;
  flex-direction: column;
  gap: 24px;
  padding: 24px;
  font-family: "Inter", system-ui, sans-serif;
  background: #f6f8fb;
  color: #1b1f2a;
}

.gantt-editor__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #ffffff;
  padding: 20px 24px;
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(31, 43, 55, 0.08);
}

.gantt-editor__eyebrow {
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: #6c7a89;
  margin: 0 0 4px;
}

.gantt-editor__title {
  margin: 0;
  font-size: 24px;
}

.gantt-editor__actions {
  display: flex;
  gap: 12px;
}

.gantt-editor__workspace {
  display: grid;
  grid-template-columns: minmax(320px, 1fr) minmax(380px, 2fr);
  gap: 24px;
}

.gantt-editor__grid {
  background: #ffffff;
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(31, 43, 55, 0.08);
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: 520px;
  overflow: auto;
}

.grid-header,
.grid-row {
  display: grid;
  grid-template-columns: 72px 240px 170px 140px 140px 70px 220px;
  gap: 8px;
  align-items: center;
  min-width: 1050px;
}

.grid-header {
  font-size: 12px;
  font-weight: 600;
  color: #5a6b7b;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  position: sticky;
  top: 0;
  background: #ffffff;
  padding: 8px 0;
  z-index: 1;
}

.grid-row {
  background: #f5f7fa;
  border-radius: 12px;
  padding: 8px;
}

.grid-cell {
  font-weight: 600;
  font-size: 13px;
  color: #2a3240;
}

.grid-input {
  border: 1px solid #d9e1eb;
  border-radius: 8px;
  padding: 6px 8px;
  font-size: 13px;
  width: 100%;
  background: #ffffff;
  min-width: 0;
}

.grid-name {
  display: flex;
  align-items: center;
  width: 100%;
  min-width: 0;
}

.grid-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.gantt-editor__timeline-wrapper {
  overflow-x: auto;
  padding-bottom: 8px;
}

.gantt-editor__timeline {
  display: flex;
  flex-direction: column;
  background: #ffffff;
  border-radius: 16px;
  padding: 16px;
  box-shadow: 0 10px 30px rgba(31, 43, 55, 0.08);
  min-width: max-content;
}

.timeline-header {
  display: grid;
  gap: 0;
  border-bottom: 1px solid #e1e7ef;
  padding-bottom: 12px;
  margin-bottom: 16px;
}

.timeline-header__cell {
  display: flex;
  flex-direction: column;
  align-items: center;
  font-size: 11px;
  color: #5f6f82;
}

.timeline-header__day {
  font-weight: 600;
  font-size: 12px;
}

.timeline-body {
  display: grid;
  gap: 12px;
  position: relative;
  grid-auto-rows: 44px;
}

.timeline-row {
  grid-column: 1 / -1;
  position: relative;
  display: grid;
  grid-template-columns: inherit;
  align-items: center;
  height: 100%;
  background: repeating-linear-gradient(
    90deg,
    #f2f5f9 0,
    #f2f5f9 calc(100% / 2),
    #ffffff calc(100% / 2),
    #ffffff calc(100% / 1)
  );
  border-radius: 12px;
  overflow: hidden;
}

.timeline-row__bar {
  position: relative;
  align-self: center;
  height: calc(100% - 14px);
  border-radius: 10px;
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 0 12px;
  color: #0b1b3f;
  font-size: 12px;
  font-weight: 600;
  box-shadow: inset 0 0 0 1px rgba(46, 91, 255, 0.18);
}

.timeline-row__bar small {
  font-size: 10px;
  font-weight: 500;
}

.btn {
  border: none;
  border-radius: 10px;
  padding: 8px 14px;
  font-size: 13px;
  cursor: pointer;
  font-weight: 600;
  transition: all 0.2s ease;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn--primary {
  background: #2f80ed;
  color: #ffffff;
  box-shadow: 0 6px 14px rgba(47, 128, 237, 0.3);
}

.btn--primary:hover {
  background: #2466c5;
}

.btn--ghost {
  background: #eef2f7;
  color: #3b4a5a;
}

.btn--ghost:hover {
  background: #dfe6f1;
}

@media (max-width: 1200px) {
  .gantt-editor__workspace {
    grid-template-columns: 1fr;
  }
}
</style>
