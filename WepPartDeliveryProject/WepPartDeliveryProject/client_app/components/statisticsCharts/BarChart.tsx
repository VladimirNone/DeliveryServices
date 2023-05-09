import { ChartData } from "chart.js";
import { FC, useEffect, useState } from "react";
import { Line } from "react-chartjs-2";
import ChartDataLabels from 'chartjs-plugin-datalabels';

import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
  } from 'chart.js';

  ChartJS.register(
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
  );

const BarChart: FC<{ query:statisticQueryInfo }> = ({ query }) => {
    const [chartData, setChartData] = useState<ChartData<"line">>();

    useEffect(() => {
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/statistic/${query.linkToQuery}`, {
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                },
            });
            const resInfo = await resp.json() as statisticQueryDataItem[];
    
            const data = {
                labels: resInfo.map((value) => value.x),
                datasets: [
                    {
                        label: query.nameQuery,
                        data: resInfo.map((value) => value.y),
                        borderColor: 'rgb(255, 99, 132)',
                        backgroundColor: '#7E07A9',
                    },
                ],
            };

            if(query.twoDataset)
            {
                data.datasets[0].label = query.nameDatasets[0];
                data.datasets.push({
                    label: query.nameDatasets[1],
                        data: resInfo.map((value) => value.y2 as number),
                        borderColor: 'rgb(255, 99, 132)',
                        backgroundColor: '#7E07A9',
                })
            }
            setChartData(data);
        }
        fetchData();
    }, [query]);

    const options = {
        plugins: {
            datalabels: {
                color: '#000',
                font: {
                    weight: 'bold' as 'bold'
                },
                align: 'left' as 'left'
              }
        },
    };

    return (
        <>
            {chartData != undefined && <Line plugins={[ChartDataLabels]} options={options} data={chartData}/>}
        </>    
    );
}

export default BarChart;