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
          <input v-model="task.name" class="grid-input" />
          <input v-model="task.owner" class="grid-input" />
          <input v-model="task.start" type="date" class="grid-input" />
          <input v-model="task.end" type="date" class="grid-input" />
          <input v-model.number="task.progress" type="number" min="0" max="100" class="grid-input" />
          <button class="btn btn--ghost" type="button" @click="removeTask(task.id)">
            Remover
          </button>
        </div>
      </aside>

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

    <footer class="gantt-editor__footer">
      <div class="gantt-editor__legend">
        <span class="legend-item">
          <span class="legend-dot legend-dot--today"></span>
          Hoje
        </span>
        <span class="legend-item">
          <span class="legend-dot legend-dot--baseline"></span>
          Linha base
        </span>
      </div>
      <div class="gantt-editor__dependencies">
        <h2>Dependências</h2>
        <div
          v-for="dependency in dependencies"
          :key="dependency.id"
          class="dependency-row"
        >
          <select v-model="dependency.from" class="grid-input">
            <option v-for="task in tasks" :key="task.id" :value="task.id">
              {{ task.code }} - {{ task.name }}
            </option>
          </select>
          <span class="dependency-arrow">→</span>
          <select v-model="dependency.to" class="grid-input">
            <option v-for="task in tasks" :key="task.id" :value="task.id">
              {{ task.code }} - {{ task.name }}
            </option>
          </select>
          <button class="btn btn--ghost" type="button" @click="removeDependency(dependency.id)">
            Excluir
          </button>
        </div>
        <button class="btn btn--ghost" type="button" @click="addDependency">
          Nova dependência
        </button>
      </div>
    </footer>
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

const tasks = ref(structuredClone(props.initialTasks));
const dependencies = ref([
  { id: 1, from: 1, to: 2 },
  { id: 2, from: 2, to: 3 },
]);

const zoomLevel = ref(1);

const timeline = computed(() => {
  const start = new Date("2024-09-01");
  const end = new Date("2024-10-15");
  const days = [];
  const current = new Date(start);

  while (current <= end) {
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
  });
};

const removeTask = (id) => {
  tasks.value = tasks.value.filter((task) => task.id !== id);
  dependencies.value = dependencies.value.filter(
    (dependency) => dependency.from !== id && dependency.to !== id,
  );
};

const addDependency = () => {
  const nextId = Math.max(0, ...dependencies.value.map((dependency) => dependency.id)) + 1;
  const [firstTask, secondTask] = tasks.value;
  dependencies.value.push({
    id: nextId,
    from: firstTask?.id ?? 0,
    to: secondTask?.id ?? 0,
  });
};

const removeDependency = (id) => {
  dependencies.value = dependencies.value.filter((dependency) => dependency.id !== id);
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
  grid-template-columns: 60px 1.5fr 1fr 1fr 1fr 60px 110px;
  gap: 8px;
  align-items: center;
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
}

.gantt-editor__timeline {
  display: flex;
  flex-direction: column;
  background: #ffffff;
  border-radius: 16px;
  padding: 16px;
  box-shadow: 0 10px 30px rgba(31, 43, 55, 0.08);
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

.gantt-editor__footer {
  display: grid;
  grid-template-columns: 1fr 2fr;
  gap: 24px;
}

.gantt-editor__legend,
.gantt-editor__dependencies {
  background: #ffffff;
  border-radius: 16px;
  padding: 16px 20px;
  box-shadow: 0 10px 30px rgba(31, 43, 55, 0.08);
}

.gantt-editor__legend {
  display: flex;
  flex-direction: column;
  gap: 12px;
  font-size: 13px;
  color: #4b5b6b;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 10px;
}

.legend-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  display: inline-block;
}

.legend-dot--today {
  background: #2f80ed;
}

.legend-dot--baseline {
  background: #7ed321;
}

.gantt-editor__dependencies h2 {
  margin: 0 0 12px;
  font-size: 16px;
}

.dependency-row {
  display: grid;
  grid-template-columns: 1fr auto 1fr auto;
  gap: 8px;
  align-items: center;
  margin-bottom: 10px;
}

.dependency-arrow {
  font-size: 16px;
  color: #6a7a8c;
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

  .gantt-editor__footer {
    grid-template-columns: 1fr;
  }
}
</style>
